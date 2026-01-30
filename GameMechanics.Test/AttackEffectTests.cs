using Csla;
using Csla.Configuration;
using GameMechanics.Combat;
using GameMechanics.Combat.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class AttackEffectTests : TestBase
{
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        services.AddGameMechanics();
    }

    #region AttackEffectGrant Tests

    [TestMethod]
    public void AttackEffectGrant_CreateBonusDamage_CreatesCorrectGrant()
    {
        var grant = AttackEffectGrant.CreateBonusDamage(3, DamageType.Energy, "Fire Sword");

        Assert.AreEqual("Energy Damage", grant.EffectName);
        Assert.AreEqual(3, grant.BonusDamage);
        Assert.AreEqual(DamageType.Energy, grant.BonusDamageType);
        Assert.AreEqual("Fire Sword", grant.Source);
    }

    [TestMethod]
    public void AttackEffectGrant_CreateDotEffect_CreatesCorrectGrant()
    {
        var grant = AttackEffectGrant.CreateDotEffect(
            "Burning",
            damagePerRound: 2,
            DamageType.Energy,
            durationRounds: 3,
            "Incendiary Ammo");

        Assert.AreEqual("Burning", grant.EffectName);
        Assert.AreEqual(EffectType.Debuff, grant.EffectType);
        Assert.AreEqual(3, grant.DurationRounds);
        Assert.AreEqual("Incendiary Ammo", grant.Source);
        Assert.IsNotNull(grant.BehaviorState);

        // Verify the behavior state contains the DoT config
        var dotState = JsonSerializer.Deserialize<DotEffectState>(grant.BehaviorState!);
        Assert.AreEqual(2, dotState!.DamagePerRound);
        Assert.AreEqual(DamageType.Energy, dotState.DamageType);
    }

    [TestMethod]
    public void AttackEffectGrant_CreateDebuff_CreatesCorrectGrant()
    {
        var grant = AttackEffectGrant.CreateDebuff(
            "Stunned",
            "Cannot take actions",
            null,
            durationRounds: 1,
            "Thunder Strike");

        Assert.AreEqual("Stunned", grant.EffectName);
        Assert.AreEqual("Cannot take actions", grant.Description);
        Assert.AreEqual(EffectType.Debuff, grant.EffectType);
        Assert.AreEqual(1, grant.DurationRounds);
        Assert.AreEqual("Thunder Strike", grant.Source);
    }

    [TestMethod]
    public void AttackEffectGrant_DefaultTriggerChance_IsOneHundredPercent()
    {
        var grant = new AttackEffectGrant
        {
            EffectName = "Test"
        };

        Assert.AreEqual(1.0, grant.TriggerChance);
    }

    [TestMethod]
    public void AttackEffectGrant_CriticalOnly_DefaultsFalse()
    {
        var grant = new AttackEffectGrant
        {
            EffectName = "Test"
        };

        Assert.IsFalse(grant.CriticalOnly);
    }

    #endregion

    #region AttackEffectContext Tests

    [TestMethod]
    public void AttackEffectContext_IsCritical_TrueAtThreshold()
    {
        Assert.IsTrue(AttackEffectContext.IsCritical(8));
        Assert.IsTrue(AttackEffectContext.IsCritical(10));
        Assert.IsFalse(AttackEffectContext.IsCritical(7));
    }

    [TestMethod]
    public void AttackEffectContext_IsCritical_CustomThreshold()
    {
        Assert.IsTrue(AttackEffectContext.IsCritical(5, threshold: 5));
        Assert.IsFalse(AttackEffectContext.IsCritical(4, threshold: 5));
    }

    #endregion

    #region AttackEffectCollectionResult Tests

    [TestMethod]
    public void AttackEffectCollectionResult_Empty_HasNoEffects()
    {
        var result = AttackEffectCollectionResult.Empty();

        Assert.IsFalse(result.HasEffects);
        Assert.AreEqual(0, result.TargetEffects.Count);
        Assert.AreEqual(0, result.AttackerEffects.Count);
        Assert.AreEqual(0, result.BonusDamage.Count);
        Assert.AreEqual(0, result.TotalBonusDamage);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_AddTargetEffect_AddsToList()
    {
        var result = new AttackEffectCollectionResult();
        var grant = new AttackEffectGrant
        {
            EffectName = "Burning",
            Description = "Target is on fire",
            DurationRounds = 3
        };

        result.AddTargetEffect(grant, "Fire Sword");

        Assert.IsTrue(result.HasEffects);
        Assert.AreEqual(1, result.TargetEffects.Count);
        Assert.AreEqual("Burning", result.TargetEffects[0].Grant.EffectName);
        Assert.AreEqual("Fire Sword", result.TargetEffects[0].SourceName);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_AddTargetEffect_AddsBonusDamageIfPresent()
    {
        var result = new AttackEffectCollectionResult();
        var grant = new AttackEffectGrant
        {
            EffectName = "Fire Strike",
            BonusDamage = 3,
            BonusDamageType = DamageType.Energy
        };

        result.AddTargetEffect(grant, "Fire Sword");

        Assert.AreEqual(1, result.BonusDamage.Count);
        Assert.AreEqual(3, result.BonusDamage[0].Damage);
        Assert.AreEqual(DamageType.Energy, result.BonusDamage[0].DamageType);
        Assert.AreEqual(3, result.TotalBonusDamage);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_AddAttackerEffect_AddsToAttackerList()
    {
        var result = new AttackEffectCollectionResult();
        var grant = new AttackEffectGrant
        {
            EffectName = "Life Drain",
            Description = "Heals attacker",
            AppliesToAttacker = true
        };

        result.AddAttackerEffect(grant, "Vampire Sword");

        Assert.IsTrue(result.HasEffects);
        Assert.AreEqual(1, result.AttackerEffects.Count);
        Assert.AreEqual("Life Drain", result.AttackerEffects[0].Grant.EffectName);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_AddBonusDamage_AddsToBonusDamageList()
    {
        var result = new AttackEffectCollectionResult();

        result.AddBonusDamage(5, DamageType.Energy, "Fire Enchantment");

        Assert.AreEqual(1, result.BonusDamage.Count);
        Assert.AreEqual(5, result.BonusDamage[0].Damage);
        Assert.AreEqual(5, result.TotalBonusDamage);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_TotalBonusDamage_SumsAllSources()
    {
        var result = new AttackEffectCollectionResult();

        result.AddBonusDamage(3, DamageType.Energy, "Fire");
        result.AddBonusDamage(2, DamageType.Energy, "Lightning");
        result.AddBonusDamage(1, DamageType.Piercing, "Sharp");

        Assert.AreEqual(6, result.TotalBonusDamage);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_Merge_CombinesResults()
    {
        var result1 = new AttackEffectCollectionResult();
        result1.AddBonusDamage(3, DamageType.Energy, "Fire");

        var result2 = new AttackEffectCollectionResult();
        result2.AddBonusDamage(2, DamageType.Energy, "Ice");
        result2.AddTargetEffect(new AttackEffectGrant { EffectName = "Frozen" }, "Ice Sword");

        result1.Merge(result2);

        Assert.AreEqual(2, result1.BonusDamage.Count);
        Assert.AreEqual(5, result1.TotalBonusDamage);
        Assert.AreEqual(1, result1.TargetEffects.Count);
    }

    [TestMethod]
    public void AttackEffectCollectionResult_GetSummary_DescribesEffects()
    {
        var result = new AttackEffectCollectionResult();
        result.AddBonusDamage(3, DamageType.Energy, "Fire");
        result.AddTargetEffect(new AttackEffectGrant { EffectName = "Burning", DurationRounds = 3 }, "Fire Sword");

        var summary = result.GetSummary();

        Assert.IsTrue(summary.Contains("Bonus damage"));
        Assert.IsTrue(summary.Contains("Target effects"));
        Assert.IsTrue(summary.Contains("Burning"));
    }

    [TestMethod]
    public void AttackEffectCollectionResult_GetSummary_EmptyReturnsNoEffects()
    {
        var result = AttackEffectCollectionResult.Empty();

        var summary = result.GetSummary();

        Assert.AreEqual("No special effects triggered.", summary);
    }

    #endregion

    #region AmmunitionProperties Effect Tests

    [TestMethod]
    public void AmmunitionProperties_GetOnHitEffect_ReturnsStructuredEffect()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            OnHitEffect = new AttackEffectGrant
            {
                EffectName = "Custom Burning",
                DurationRounds = 5,
                BonusDamage = 2,
                BonusDamageType = DamageType.Energy
            }
        };

        var effect = props.GetOnHitEffect();

        Assert.IsNotNull(effect);
        Assert.AreEqual("Custom Burning", effect.EffectName);
        Assert.AreEqual(5, effect.DurationRounds);
        Assert.AreEqual(2, effect.BonusDamage);
    }

    [TestMethod]
    public void AmmunitionProperties_GetOnHitEffect_FallsBackToSpecialEffect()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Incendiary"
        };

        var effect = props.GetOnHitEffect();

        Assert.IsNotNull(effect);
        Assert.AreEqual("Burning", effect.EffectName);
        Assert.AreEqual(3, effect.DurationRounds);
    }

    [TestMethod]
    public void AmmunitionProperties_GetParsedSpecialEffect_Incendiary()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Incendiary"
        };

        var effect = props.GetParsedSpecialEffect();

        Assert.IsNotNull(effect);
        Assert.AreEqual("Burning", effect.EffectName);
        Assert.AreEqual(DamageType.Energy, ((DotEffectState)JsonSerializer.Deserialize<DotEffectState>(effect.BehaviorState!)!).DamageType);
    }

    [TestMethod]
    public void AmmunitionProperties_GetParsedSpecialEffect_Shock()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Shock"
        };

        var effect = props.GetParsedSpecialEffect();

        Assert.IsNotNull(effect);
        Assert.AreEqual("Shocked", effect.EffectName);
        Assert.AreEqual(1, effect.BonusDamage);
        Assert.AreEqual(DamageType.Energy, effect.BonusDamageType);
    }

    [TestMethod]
    public void AmmunitionProperties_GetParsedSpecialEffect_HollowPoint()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Hollow-Point"
        };

        var effect = props.GetParsedSpecialEffect();

        Assert.IsNotNull(effect);
        Assert.AreEqual("Expanded Wound", effect.EffectName);
        Assert.AreEqual(2, effect.BonusDamage);
        Assert.AreEqual(DamageType.Projectile, effect.BonusDamageType);
    }

    [TestMethod]
    public void AmmunitionProperties_GetParsedSpecialEffect_ArmorPiercing_ReturnsNull()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Armor-Piercing"
        };

        var effect = props.GetParsedSpecialEffect();

        // AP is handled by penetration modifier, not an effect
        Assert.IsNull(effect);
    }

    [TestMethod]
    public void AmmunitionProperties_GetParsedSpecialEffect_NullSpecialEffect_ReturnsNull()
    {
        var props = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = null
        };

        var effect = props.GetParsedSpecialEffect();

        Assert.IsNull(effect);
    }

    #endregion

    #region SpellBuffState Attack Buff Tests

    [TestMethod]
    public void SpellBuffState_CreateAttackBuff_WithOnHitEffect()
    {
        var onHitEffect = AttackEffectGrant.CreateDotEffect(
            "Burning",
            damagePerRound: 2,
            DamageType.Energy,
            durationRounds: 3,
            "Fire Punch");

        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Fire Punch",
            durationRounds: 50,
            onHitEffect: onHitEffect);

        Assert.AreEqual("Fire Punch", buffState.BuffName);
        Assert.AreEqual(50, buffState.TotalDurationRounds);
        Assert.AreEqual(1, buffState.Modifiers.Count);
        Assert.AreEqual(BuffModifierType.OnHitEffect, buffState.Modifiers[0].Type);
        Assert.IsNotNull(buffState.Modifiers[0].OnHitEffectGrant);
    }

    [TestMethod]
    public void SpellBuffState_CreateAttackBuff_WithBonusDamage()
    {
        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Fire Enchantment",
            durationRounds: 100,
            bonusDamage: 3,
            bonusDamageType: DamageType.Energy);

        Assert.AreEqual("Fire Enchantment", buffState.BuffName);
        Assert.AreEqual(1, buffState.Modifiers.Count);
        Assert.AreEqual(BuffModifierType.BonusDamage, buffState.Modifiers[0].Type);
        Assert.AreEqual(3, buffState.Modifiers[0].Value);
        Assert.AreEqual(DamageType.Energy, buffState.Modifiers[0].BonusDamageType);
    }

    [TestMethod]
    public void SpellBuffState_CreateAttackBuff_WithBothEffectAndDamage()
    {
        var onHitEffect = new AttackEffectGrant
        {
            EffectName = "Frozen",
            DurationRounds = 2
        };

        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Frost Strike",
            durationRounds: 60,
            onHitEffect: onHitEffect,
            bonusDamage: 2,
            bonusDamageType: DamageType.Energy);

        Assert.AreEqual(2, buffState.Modifiers.Count);
        Assert.IsTrue(buffState.Modifiers.Any(m => m.Type == BuffModifierType.OnHitEffect));
        Assert.IsTrue(buffState.Modifiers.Any(m => m.Type == BuffModifierType.BonusDamage));
    }

    [TestMethod]
    public void SpellBuffState_GetOnHitEffects_ReturnsOnHitGrants()
    {
        var onHitEffect = new AttackEffectGrant
        {
            EffectName = "Burning",
            DurationRounds = 3
        };

        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Fire Punch",
            durationRounds: 50,
            onHitEffect: onHitEffect);

        var effects = buffState.GetOnHitEffects().ToList();

        Assert.AreEqual(1, effects.Count);
        Assert.AreEqual("Burning", effects[0].EffectName);
    }

    [TestMethod]
    public void SpellBuffState_GetBonusDamage_ReturnsBonusDamageGrants()
    {
        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Fire Enchantment",
            durationRounds: 100,
            bonusDamage: 3,
            bonusDamageType: DamageType.Energy);

        var damages = buffState.GetBonusDamage().ToList();

        Assert.AreEqual(1, damages.Count);
        Assert.AreEqual(3, damages[0].Damage);
        Assert.AreEqual(DamageType.Energy, damages[0].Type);
    }

    [TestMethod]
    public void SpellBuffState_Serialize_RoundTripsAttackBuff()
    {
        var onHitEffect = new AttackEffectGrant
        {
            EffectName = "Burning",
            DurationRounds = 3,
            BonusDamage = 2,
            BonusDamageType = DamageType.Energy
        };

        var original = SpellBuffState.CreateAttackBuff(
            name: "Fire Punch",
            durationRounds: 50,
            onHitEffect: onHitEffect,
            bonusDamage: 1,
            bonusDamageType: DamageType.Energy);

        var json = original.Serialize();
        var restored = SpellBuffState.Deserialize(json);

        Assert.AreEqual(original.BuffName, restored.BuffName);
        Assert.AreEqual(original.TotalDurationRounds, restored.TotalDurationRounds);
        Assert.AreEqual(2, restored.Modifiers.Count);

        var restoredOnHit = restored.GetOnHitEffects().FirstOrDefault();
        Assert.IsNotNull(restoredOnHit);
        Assert.AreEqual("Burning", restoredOnHit.EffectName);
    }

    #endregion

    #region BuffModifier Attack Effect Tests

    [TestMethod]
    public void BuffModifier_OnHitEffect_StoresGrant()
    {
        var grant = new AttackEffectGrant
        {
            EffectName = "Poison",
            DurationRounds = 5
        };

        var modifier = new BuffModifier
        {
            Type = BuffModifierType.OnHitEffect,
            Target = "Attack",
            OnHitEffectGrant = grant
        };

        Assert.AreEqual(BuffModifierType.OnHitEffect, modifier.Type);
        Assert.IsNotNull(modifier.OnHitEffectGrant);
        Assert.AreEqual("Poison", modifier.OnHitEffectGrant.EffectName);
    }

    [TestMethod]
    public void BuffModifier_BonusDamage_StoresTypeAndValue()
    {
        var modifier = new BuffModifier
        {
            Type = BuffModifierType.BonusDamage,
            Target = "Attack",
            Value = 5,
            BonusDamageType = DamageType.Energy
        };

        Assert.AreEqual(BuffModifierType.BonusDamage, modifier.Type);
        Assert.AreEqual(5, modifier.Value);
        Assert.AreEqual(DamageType.Energy, modifier.BonusDamageType);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void AttackEffectService_Registered_InServiceCollection()
    {
        var provider = InitServices();

        var service = provider.GetService<AttackEffectService>();

        Assert.IsNotNull(service);
    }

    [TestMethod]
    public async Task AttackEffectService_CollectAttackEffectsAsync_CollectsBuffEffects()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        // Add an attack buff to the attacker
        var buffState = SpellBuffState.CreateAttackBuff(
            name: "Fire Punch",
            durationRounds: 50,
            bonusDamage: 3,
            bonusDamageType: DamageType.Energy);

        var buffEffect = effectPortal.CreateChild(
            EffectType.Buff,
            buffState.BuffName,
            null,
            buffState.TotalDurationRounds,
            buffState.Serialize());

        attacker.Effects.AddEffect(buffEffect);

        var context = new AttackEffectContext
        {
            Attacker = attacker,
            Target = target,
            AttackSV = 5,
            HitLocation = HitLocation.Torso,
            IsCriticalHit = false,
            BaseDamageType = DamageType.Bashing
        };

        var result = await service.CollectAttackEffectsAsync(context);

        Assert.IsTrue(result.HasEffects);
        Assert.AreEqual(3, result.TotalBonusDamage);
        Assert.AreEqual(1, result.BonusDamage.Count);
        Assert.AreEqual("Fire Punch", result.BonusDamage[0].Source);
    }

    [TestMethod]
    public async Task AttackEffectService_CollectAttackEffectsAsync_CollectsWeaponOnAttackEffects()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        var weaponTemplate = new ItemTemplate
        {
            Id = 1,
            Name = "Flaming Sword",
            Effects =
            [
                new ItemEffectDefinition
                {
                    Id = 1,
                    Name = "Flame Burst",
                    EffectType = EffectType.Debuff,
                    Trigger = ItemEffectTrigger.OnAttackWith,
                    DurationRounds = 2,
                    IsActive = true,
                    BehaviorState = JsonSerializer.Serialize(new { bonusDamage = 2, bonusDamageType = "Energy" })
                }
            ]
        };

        var context = new AttackEffectContext
        {
            Attacker = attacker,
            Target = target,
            WeaponItemId = Guid.NewGuid(),
            WeaponTemplate = weaponTemplate,
            AttackSV = 5,
            HitLocation = HitLocation.Torso,
            IsCriticalHit = false,
            BaseDamageType = DamageType.Cutting
        };

        var result = await service.CollectAttackEffectsAsync(context);

        Assert.IsTrue(result.HasEffects);
        Assert.AreEqual(1, result.TargetEffects.Count);
        Assert.AreEqual("Flame Burst", result.TargetEffects[0].Grant.EffectName);
    }

    [TestMethod]
    public async Task AttackEffectService_CollectAttackEffectsAsync_CollectsOnCriticalEffectsOnlyOnCrit()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        var weaponTemplate = new ItemTemplate
        {
            Id = 1,
            Name = "Critical Sword",
            Effects =
            [
                new ItemEffectDefinition
                {
                    Id = 1,
                    Name = "Critical Strike",
                    EffectType = EffectType.Debuff,
                    Trigger = ItemEffectTrigger.OnCritical,
                    DurationRounds = 3,
                    IsActive = true
                }
            ]
        };

        // Non-critical hit - should not trigger
        var nonCritContext = new AttackEffectContext
        {
            Attacker = attacker,
            Target = target,
            WeaponTemplate = weaponTemplate,
            AttackSV = 5,
            HitLocation = HitLocation.Torso,
            IsCriticalHit = false,
            BaseDamageType = DamageType.Cutting
        };

        var nonCritResult = await service.CollectAttackEffectsAsync(nonCritContext);
        Assert.IsFalse(nonCritResult.HasEffects);

        // Critical hit - should trigger
        var critContext = new AttackEffectContext
        {
            Attacker = attacker,
            Target = target,
            WeaponTemplate = weaponTemplate,
            AttackSV = 10,
            HitLocation = HitLocation.Torso,
            IsCriticalHit = true,
            BaseDamageType = DamageType.Cutting
        };

        var critResult = await service.CollectAttackEffectsAsync(critContext);
        Assert.IsTrue(critResult.HasEffects);
        Assert.AreEqual(1, critResult.TargetEffects.Count);
        Assert.AreEqual("Critical Strike", critResult.TargetEffects[0].Grant.EffectName);
    }

    [TestMethod]
    public async Task AttackEffectService_CollectAttackEffectsAsync_RangedContext_CollectsAmmoEffects()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        var ammoProps = new AmmunitionProperties
        {
            IsAmmunition = true,
            AmmoType = "9mm",
            SpecialEffect = "Incendiary",
            DamageModifier = 1
        };

        var context = new RangedAttackEffectContext
        {
            Attacker = attacker,
            Target = target,
            AmmunitionItemId = Guid.NewGuid(),
            AmmoProperties = ammoProps,
            AttackSV = 5,
            HitLocation = HitLocation.Torso,
            IsCriticalHit = false,
            BaseDamageType = DamageType.Projectile
        };

        var result = await service.CollectAttackEffectsAsync(context);

        Assert.IsTrue(result.HasEffects);
        // Should have burning effect from incendiary
        Assert.AreEqual(1, result.TargetEffects.Count);
        Assert.AreEqual("Burning", result.TargetEffects[0].Grant.EffectName);
        // Should have damage modifier as bonus damage
        Assert.IsTrue(result.BonusDamage.Any(b => b.Damage == 1));
    }

    [TestMethod]
    public async Task AttackEffectService_ApplyEffectsAsync_AppliesTargetEffects()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        var collectionResult = new AttackEffectCollectionResult();
        collectionResult.AddTargetEffect(
            new AttackEffectGrant
            {
                EffectName = "Burning",
                EffectType = EffectType.Debuff,
                DurationRounds = 3,
                Description = "On fire"
            },
            "Fire Sword");

        Assert.AreEqual(0, target.Effects.Count);

        await service.ApplyEffectsAsync(collectionResult, target, attacker);

        Assert.AreEqual(1, target.Effects.Count);
        Assert.AreEqual("Burning", target.Effects[0].Name);
        Assert.AreEqual(EffectType.Debuff, target.Effects[0].EffectType);
    }

    [TestMethod]
    public async Task AttackEffectService_ApplyEffectsAsync_AppliesAttackerEffects()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var service = provider.GetRequiredService<AttackEffectService>();

        var attacker = dp.Create(1);
        var target = dp.Create(2);

        var collectionResult = new AttackEffectCollectionResult();
        collectionResult.AddAttackerEffect(
            new AttackEffectGrant
            {
                EffectName = "Life Steal",
                EffectType = EffectType.Buff,
                DurationRounds = 1,
                Description = "Healed by attack",
                AppliesToAttacker = true
            },
            "Vampire Blade");

        Assert.AreEqual(0, attacker.Effects.Count);

        await service.ApplyEffectsAsync(collectionResult, target, attacker);

        Assert.AreEqual(1, attacker.Effects.Count);
        Assert.AreEqual("Life Steal", attacker.Effects[0].Name);
    }

    #endregion
}
