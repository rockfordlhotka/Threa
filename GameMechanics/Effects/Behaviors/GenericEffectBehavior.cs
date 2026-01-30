using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Generic effect behavior that applies modifiers from EffectState.
/// Designed for user-created effects from templates that use the simpler
/// EffectState serialization rather than specialized state classes.
///
/// This behavior handles:
/// - Attribute modifiers (e.g., STR +2, DEX -1)
/// - Skill modifiers (e.g., Perception +2)
/// - Global AS modifier (applies to all ability checks)
/// - Per-tick damage (FAT/VIT)
/// - Per-tick healing (FAT/VIT)
///
/// Note: EffectType.Buff, EffectType.Debuff, and EffectType.Condition already
/// have specialized behaviors. This behavior is available for types that don't
/// have dedicated behaviors or can be explicitly registered for specific types.
/// </summary>
public class GenericEffectBehavior : IEffectBehavior
{
    /// <summary>
    /// Default effect type for this behavior.
    /// This behavior can handle multiple types via explicit registration.
    /// </summary>
    public EffectType EffectType => EffectType.Buff;

    /// <summary>
    /// Allows the effect to be added. Stacking is allowed by default.
    /// </summary>
    public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
    {
        // Allow stacking by default - effects from templates can coexist
        return EffectAddResult.AddNormally();
    }

    /// <summary>
    /// No immediate effect on apply - modifiers are continuous.
    /// </summary>
    public void OnApply(EffectRecord effect, CharacterEdit character)
    {
        // Modifiers are applied continuously via GetAttributeModifiers/GetAbilityScoreModifiers
        // No immediate effect needed
    }

    /// <summary>
    /// Applies per-tick damage and healing from EffectState.
    /// </summary>
    public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
    {
        var state = EffectState.Deserialize(effect.BehaviorState);

        // Apply FAT damage per tick
        if (state.FatDamagePerTick.HasValue && state.FatDamagePerTick.Value > 0)
        {
            character.Fatigue.PendingDamage += state.FatDamagePerTick.Value;
        }

        // Apply VIT damage per tick
        if (state.VitDamagePerTick.HasValue && state.VitDamagePerTick.Value > 0)
        {
            character.Vitality.PendingDamage += state.VitDamagePerTick.Value;
        }

        // Apply FAT healing per tick
        if (state.FatHealingPerTick.HasValue && state.FatHealingPerTick.Value > 0)
        {
            character.Fatigue.PendingHealing += state.FatHealingPerTick.Value;
        }

        // Apply VIT healing per tick
        if (state.VitHealingPerTick.HasValue && state.VitHealingPerTick.Value > 0)
        {
            character.Vitality.PendingHealing += state.VitHealingPerTick.Value;
        }

        return EffectTickResult.Continue();
    }

    /// <summary>
    /// No cleanup needed on expiration.
    /// </summary>
    public void OnExpire(EffectRecord effect, CharacterEdit character)
    {
        // No special cleanup - modifiers naturally stop applying
    }

    /// <summary>
    /// No cleanup needed on removal.
    /// </summary>
    public void OnRemove(EffectRecord effect, CharacterEdit character)
    {
        // No special cleanup - modifiers naturally stop applying
    }

    /// <summary>
    /// Gets attribute modifiers from the EffectState.
    /// </summary>
    public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
    {
        var state = EffectState.Deserialize(effect.BehaviorState);

        // Check if this attribute has a modifier
        var modifier = state.GetAttributeModifier(attributeName);
        if (modifier != 0)
        {
            yield return new EffectModifier
            {
                Description = effect.Name,
                Value = modifier,
                TargetAttribute = attributeName
            };
        }
    }

    /// <summary>
    /// Gets ability score modifiers from the EffectState.
    /// Applies global AS modifier and skill-specific modifiers.
    /// </summary>
    public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
    {
        var state = EffectState.Deserialize(effect.BehaviorState);

        // Apply global AS modifier (affects all ability checks)
        if (state.ASModifier.HasValue && state.ASModifier.Value != 0)
        {
            yield return new EffectModifier
            {
                Description = effect.Name,
                Value = state.ASModifier.Value,
                TargetSkill = skillName
            };
        }

        // Apply skill-specific modifier
        var skillModifier = state.GetSkillModifier(skillName);
        if (skillModifier != 0)
        {
            yield return new EffectModifier
            {
                Description = $"{effect.Name} ({skillName})",
                Value = skillModifier,
                TargetSkill = skillName
            };
        }
    }

    /// <summary>
    /// Generic effects don't modify success values directly.
    /// </summary>
    public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
    {
        // Generic effects work through AS modifiers, not SV modifiers
        return [];
    }
}
