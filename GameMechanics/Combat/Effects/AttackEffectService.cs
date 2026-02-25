using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Effects.Behaviors;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Combat.Effects;

/// <summary>
/// Service for collecting and applying attack-triggered effects.
/// Gathers effects from weapons, ammunition, buffs, and equipped items
/// that trigger when an attack hits.
/// </summary>
public class AttackEffectService
{
    private readonly IChildDataPortal<EffectRecord> _effectPortal;
    private readonly IItemTemplateDal _templateDal;
    private readonly ICharacterItemDal _itemDal;

    public AttackEffectService(
        IChildDataPortal<EffectRecord> effectPortal,
        IItemTemplateDal templateDal,
        ICharacterItemDal itemDal)
    {
        _effectPortal = effectPortal;
        _templateDal = templateDal;
        _itemDal = itemDal;
    }

    /// <summary>
    /// Collects all attack effects that should trigger for the given attack context.
    /// </summary>
    /// <param name="context">The attack context containing attacker, weapon, and hit information.</param>
    /// <returns>A collection result containing all triggered effects.</returns>
    public async Task<AttackEffectCollectionResult> CollectAttackEffectsAsync(AttackEffectContext context)
    {
        var result = new AttackEffectCollectionResult();

        // 1. Collect weapon effects (OnAttackWith, OnCritical)
        await CollectWeaponEffectsAsync(context, result);

        // 2. Collect ammunition effects (for ranged attacks)
        if (context is RangedAttackEffectContext rangedContext)
        {
            CollectAmmunitionEffects(rangedContext, result);
        }

        // 3. Collect buff-granted attack effects
        CollectBuffEffects(context, result);

        // 4. Collect equipped item effects (OnAttackWith, OnHitWhileWearing triggers)
        await CollectEquippedItemEffectsAsync(context, result);

        return result;
    }

    /// <summary>
    /// Applies the collected effects to the target and/or attacker.
    /// </summary>
    /// <param name="collectionResult">The collected effects.</param>
    /// <param name="target">The target character.</param>
    /// <param name="attacker">The attacking character.</param>
    /// <returns>True if all effects were applied successfully.</returns>
    public async Task<bool> ApplyEffectsAsync(
        AttackEffectCollectionResult collectionResult,
        CharacterEdit target,
        CharacterEdit attacker)
    {
        bool allSuccess = true;

        // Apply target effects
        foreach (var effect in collectionResult.TargetEffects)
        {
            var applied = await ApplyEffectGrantAsync(effect.Grant, target, effect.SourceName);
            allSuccess &= applied;
        }

        // Apply attacker effects (life-steal, self-buffs) — skipped when attacker is unavailable.
        if (attacker != null)
        {
            foreach (var effect in collectionResult.AttackerEffects)
            {
                var applied = await ApplyEffectGrantAsync(effect.Grant, attacker, effect.SourceName);
                allSuccess &= applied;
            }
        }

        return allSuccess;
    }

    #region Private Collection Methods

    private async Task CollectWeaponEffectsAsync(AttackEffectContext context, AttackEffectCollectionResult result)
    {
        if (context.WeaponTemplate == null)
            return;

        var weaponName = context.WeaponTemplate.Name;

        foreach (var effectDef in context.WeaponTemplate.Effects)
        {
            if (!effectDef.IsActive)
                continue;

            // Check trigger conditions
            bool shouldTrigger = effectDef.Trigger switch
            {
                ItemEffectTrigger.OnAttackWith => true,
                ItemEffectTrigger.OnCritical => context.IsCriticalHit,
                _ => false
            };

            if (!shouldTrigger)
                continue;

            var grant = ConvertEffectDefinitionToGrant(effectDef, weaponName);
            if (grant != null)
            {
                if (grant.AppliesToAttacker)
                    result.AddAttackerEffect(grant, weaponName);
                else if (ShouldTargetEffectApply(grant, context))
                    result.AddTargetEffect(grant, weaponName);
            }
        }
    }

    private void CollectAmmunitionEffects(RangedAttackEffectContext context, AttackEffectCollectionResult result)
    {
        if (context.AmmoProperties == null)
            return;

        var ammoName = context.AmmunitionTemplate?.Name ?? context.AmmoProperties.AmmoType ?? "Ammo";

        // Check for structured OnHitEffect
        var onHitEffect = context.AmmoProperties.GetOnHitEffect();
        if (onHitEffect != null)
        {
            if (onHitEffect.AppliesToAttacker)
                result.AddAttackerEffect(onHitEffect, ammoName);
            else if (ShouldTargetEffectApply(onHitEffect, context))
                result.AddTargetEffect(onHitEffect, ammoName);
        }

        // Legacy: Parse SpecialEffect string for backwards compatibility
        else if (!string.IsNullOrEmpty(context.AmmoProperties.SpecialEffect))
        {
            var legacyEffect = ParseLegacySpecialEffect(context.AmmoProperties.SpecialEffect, ammoName);
            if (legacyEffect != null && ShouldTargetEffectApply(legacyEffect, context))
            {
                result.AddTargetEffect(legacyEffect, ammoName);
            }
        }

        // Add damage modifier as bonus damage if present
        if (context.AmmoProperties.DamageModifier != 0)
        {
            result.AddBonusDamage(
                context.AmmoProperties.DamageModifier,
                DamageType.Projectile,
                ammoName);
        }
    }

    private void CollectBuffEffects(AttackEffectContext context, AttackEffectCollectionResult result)
    {
        // Skip if attacker's character object isn't available.
        if (context.Attacker == null) return;

        // Get all active buffs on the attacker
        var activeBuffs = context.Attacker.Effects
            .Where(e => e.EffectType == EffectType.Buff && e.IsActive)
            .ToList();

        foreach (var buffEffect in activeBuffs)
        {
            var state = SpellBuffState.Deserialize(buffEffect.BehaviorState);

            // Check for OnHitEffect modifiers
            foreach (var onHitGrant in state.GetOnHitEffects())
            {
                // Check critical requirement
                if (onHitGrant.CriticalOnly && !context.IsCriticalHit)
                    continue;

                // Set source if not already set
                if (string.IsNullOrEmpty(onHitGrant.Source))
                    onHitGrant.Source = state.BuffName;

                if (onHitGrant.AppliesToAttacker)
                    result.AddAttackerEffect(onHitGrant, state.BuffName);
                else
                    result.AddTargetEffect(onHitGrant, state.BuffName);
            }

            // Check for BonusDamage modifiers
            foreach (var (damage, damageType) in state.GetBonusDamage())
            {
                result.AddBonusDamage(damage, damageType, state.BuffName);
            }
        }
    }

    private async Task CollectEquippedItemEffectsAsync(AttackEffectContext context, AttackEffectCollectionResult result)
    {
        // Skip if attacker's character object isn't available.
        if (context.Attacker == null) return;

        // Get all equipped items for the attacker (excluding the weapon which was already checked)
        var equippedItems = await _itemDal.GetEquippedItemsAsync(context.Attacker.Id);

        foreach (var item in equippedItems)
        {
            // Skip the weapon itself - already handled
            if (context.WeaponItemId.HasValue && item.Id == context.WeaponItemId.Value)
                continue;

            if (item.Template == null)
            {
                try
                {
                    item.Template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
                }
                catch
                {
                    continue;
                }
            }

            var itemName = item.CustomName ?? item.Template.Name;

            foreach (var effectDef in item.Template.Effects)
            {
                if (!effectDef.IsActive)
                    continue;

                // Check trigger conditions for equipped items
                bool shouldTrigger = effectDef.Trigger switch
                {
                    ItemEffectTrigger.OnAttackWith => true, // Some items grant attack effects while equipped
                    ItemEffectTrigger.OnCritical => context.IsCriticalHit,
                    _ => false
                };

                if (!shouldTrigger)
                    continue;

                var grant = ConvertEffectDefinitionToGrant(effectDef, itemName);
                if (grant != null)
                {
                    if (grant.AppliesToAttacker)
                        result.AddAttackerEffect(grant, itemName);
                    else if (ShouldTargetEffectApply(grant, context))
                        result.AddTargetEffect(grant, itemName);
                }
            }
        }
    }

    #endregion

    #region Armor / DC Filter

    /// <summary>
    /// Returns true if a target effect should be applied, taking the armor interaction
    /// rule and damage class restrictions into account.
    /// Only called for effects that target the defender (AppliesToAttacker == false).
    /// </summary>
    private static bool ShouldTargetEffectApply(AttackEffectGrant grant, AttackEffectContext context)
    {
        // Damage class check: skip if any DC (armor, shield, or target) meets or exceeds the effect's DC.
        if (grant.EffectDamageClass > 0)
        {
            if (context.ArmorDamageClass.HasValue && context.ArmorDamageClass.Value >= grant.EffectDamageClass)
                return false;
            if (context.ShieldDamageClass.HasValue && context.ShieldDamageClass.Value >= grant.EffectDamageClass)
                return false;
            if (context.TargetDamageClass > 0 && context.TargetDamageClass >= grant.EffectDamageClass)
                return false;
        }

        // Armor interaction rule check.
        return grant.ArmorRule switch
        {
            ArmorInteractionRule.PenetrationOnly =>
                // No armor at hit location OR the attack actually penetrated it.
                context.ArmorDamageClass == null || context.ArmorWasPenetrated,
            ArmorInteractionRule.NoArmor =>
                context.ArmorDamageClass == null,
            ArmorInteractionRule.IgnoreArmor =>
                true,
            _ => true
        };
    }

    #endregion

    #region Helper Methods

    private AttackEffectGrant? ConvertEffectDefinitionToGrant(ItemEffectDefinition effectDef, string sourceName)
    {
        // Try to extract bonus damage info from behavior state
        int bonusDamage = 0;
        DamageType? bonusDamageType = null;
        bool appliesToAttacker = false;

        if (!string.IsNullOrEmpty(effectDef.BehaviorState))
        {
            try
            {
                var stateDoc = JsonDocument.Parse(effectDef.BehaviorState);
                var root = stateDoc.RootElement;

                if (root.TryGetProperty("bonusDamage", out var dmgElement))
                    bonusDamage = dmgElement.GetInt32();

                if (root.TryGetProperty("bonusDamageType", out var typeElement))
                {
                    if (Enum.TryParse<DamageType>(typeElement.GetString(), true, out var parsedType))
                        bonusDamageType = parsedType;
                }

                if (root.TryGetProperty("appliesToAttacker", out var attackerElement))
                    appliesToAttacker = attackerElement.GetBoolean();
            }
            catch
            {
                // Ignore parse errors
            }
        }

        return new AttackEffectGrant
        {
            EffectName = effectDef.Name,
            Description = effectDef.Description,
            EffectType = effectDef.EffectType,
            BehaviorState = effectDef.BehaviorState,
            DurationRounds = effectDef.DurationRounds,
            BonusDamage = bonusDamage,
            BonusDamageType = bonusDamageType,
            Source = sourceName,
            AppliesToAttacker = appliesToAttacker,
            IconName = effectDef.IconName,
            ArmorRule = effectDef.ArmorRule,
            EffectDamageClass = effectDef.EffectDamageClass
        };
    }

    private AttackEffectGrant? ParseLegacySpecialEffect(string specialEffect, string sourceName)
    {
        return specialEffect.ToUpperInvariant() switch
        {
            "INCENDIARY" => AttackEffectGrant.CreateDotEffect(
                "Burning",
                damagePerRound: 2,
                DamageType.Energy,
                durationRounds: 3,
                sourceName),

            "CRYO" or "FREEZING" => AttackEffectGrant.CreateDebuff(
                "Chilled",
                "Movement and actions slowed",
                JsonSerializer.Serialize(new { MovementPenalty = -2, APPenalty = -1 }),
                durationRounds: 2,
                sourceName),

            "SHOCK" or "ELECTRIC" => new AttackEffectGrant
            {
                EffectName = "Shocked",
                Description = "Electrical damage and potential stun",
                EffectType = EffectType.Debuff,
                BonusDamage = 1,
                BonusDamageType = DamageType.Energy,
                DurationRounds = 1,
                Source = sourceName
            },

            "HOLLOW-POINT" or "HOLLOWPOINT" => new AttackEffectGrant
            {
                EffectName = "Expanded Wound",
                Description = "Increased damage to unarmored targets",
                BonusDamage = 2,
                BonusDamageType = DamageType.Projectile,
                Source = sourceName
            },

            "ARMOR-PIERCING" or "AP" => new AttackEffectGrant
            {
                EffectName = "Penetrating",
                Description = "Ignores some armor",
                Source = sourceName
                // Armor piercing is handled by penetration modifier, not damage
            },

            "TRACER" => null, // Tracer is visual only, no combat effect

            "EXPLOSIVE" or "HE" => new AttackEffectGrant
            {
                EffectName = "Explosive",
                Description = "Area damage",
                BonusDamage = 3,
                BonusDamageType = DamageType.Bashing,
                Source = sourceName
            },

            _ => null
        };
    }

    private async Task<bool> ApplyEffectGrantAsync(AttackEffectGrant grant, CharacterEdit target, string sourceName)
    {
        // Only create an effect record if there's an actual effect to apply
        // (not just bonus damage)
        if (string.IsNullOrEmpty(grant.EffectName) ||
            (grant.DurationRounds == null && grant.BonusDamage == 0 && string.IsNullOrEmpty(grant.BehaviorState)))
        {
            return true;
        }

        // Check trigger chance
        if (grant.TriggerChance < 1.0)
        {
            var random = new Random();
            if (random.NextDouble() > grant.TriggerChance)
                return true; // Didn't trigger, but that's not a failure
        }

        try
        {
            var effect = _effectPortal.CreateChild(
                grant.EffectType,
                grant.EffectName,
                null, // location
                grant.DurationRounds,
                grant.BehaviorState);

            effect.Description = grant.Description;
            effect.Source = sourceName;

            return target.Effects.AddEffect(effect);
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
