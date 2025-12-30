using System;
using System.Collections.Generic;
using GameMechanics.Magic.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

/// <summary>
/// Tests for spell effect classes (Phase 3: Individual Spells).
/// </summary>
[TestClass]
public class SpellEffectTests
{
    #region SpellEffectFactory Tests

    [TestMethod]
    public void SpellEffectFactory_CreateWithStandardEffects_RegistersAllEffects()
    {
        var factory = SpellEffectFactory.CreateWithStandardEffects();

        Assert.IsTrue(factory.HasEffect("mystic-punch"));
        Assert.IsTrue(factory.HasEffect("fire-bolt"));
        Assert.IsTrue(factory.HasEffect("minor-heal"));
        Assert.IsTrue(factory.HasEffect("illuminate-area"));
        Assert.IsTrue(factory.HasEffect("wall-of-fire"));
    }

    [TestMethod]
    public void SpellEffectFactory_GetEffect_ReturnsCorrectHandler()
    {
        var factory = SpellEffectFactory.CreateWithStandardEffects();

        var physicalEffect = factory.GetEffect("mystic-punch");
        var energyEffect = factory.GetEffect("fire-bolt");
        var healingEffect = factory.GetEffect("minor-heal");

        Assert.IsInstanceOfType(physicalEffect, typeof(PhysicalDamageSpellEffect));
        Assert.IsInstanceOfType(energyEffect, typeof(EnergyDamageSpellEffect));
        Assert.IsInstanceOfType(healingEffect, typeof(HealingSpellEffect));
    }

    [TestMethod]
    public void SpellEffectFactory_UnknownSpell_ReturnsNull()
    {
        var factory = SpellEffectFactory.CreateWithStandardEffects();

        var result = factory.GetEffect("unknown-spell");

        Assert.IsNull(result);
    }

    #endregion

    #region PhysicalDamageSpellEffect Tests

    [TestMethod]
    public void PhysicalDamageSpellEffect_SV0_DealsMinimalDamage()
    {
        var effect = new PhysicalDamageSpellEffect();
        var spell = CreateSpell("mystic-punch", SpellType.Targeted);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 0,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.DamageDealt.Count);
        Assert.AreEqual(1, result.DamageDealt[0].FatigueDamage);
        Assert.AreEqual(0, result.DamageDealt[0].VitalityDamage);
        Assert.IsFalse(result.DamageDealt[0].CausedWound);
    }

    [TestMethod]
    public void PhysicalDamageSpellEffect_HighSV_CausesWound()
    {
        var effect = new PhysicalDamageSpellEffect();
        var spell = CreateSpell("mystic-punch", SpellType.Targeted);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 6,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.DamageDealt[0].CausedWound);
        Assert.IsTrue(result.DamageDealt[0].VitalityDamage > 0);
    }

    [TestMethod]
    public void PhysicalDamageSpellEffect_WithPump_IncreasesEffectiveSV()
    {
        var effect = new PhysicalDamageSpellEffect();
        var spell = CreateSpell("mystic-punch", SpellType.Targeted);
        
        // Without pump: SV 2
        var contextNoPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2
        };
        
        // With pump: SV 2 + 3 FAT pump = effective SV 5
        var contextWithPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2,
            PumpedFatigue = 3
        };

        var resultNoPump = effect.Resolve(contextNoPump);
        var resultWithPump = effect.Resolve(contextWithPump);

        Assert.IsTrue(resultWithPump.DamageDealt[0].FatigueDamage > resultNoPump.DamageDealt[0].FatigueDamage);
    }

    [TestMethod]
    public void PhysicalDamageSpellEffect_NegativeSV_NoDamage()
    {
        var effect = new PhysicalDamageSpellEffect();
        var spell = CreateSpell("mystic-punch", SpellType.Targeted);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = -2,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.DamageDealt[0].FatigueDamage);
        Assert.AreEqual(0, result.DamageDealt[0].VitalityDamage);
    }

    [TestMethod]
    public void PhysicalDamageSpellEffect_NoTarget_Fails()
    {
        var effect = new PhysicalDamageSpellEffect();
        var spell = CreateSpell("mystic-punch", SpellType.Targeted);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1
            // No TargetCharacterId
        };

        var result = effect.Resolve(context);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
    }

    #endregion

    #region EnergyDamageSpellEffect Tests

    [TestMethod]
    public void EnergyDamageSpellEffect_FireBolt_DealsDamage()
    {
        var effect = new EnergyDamageSpellEffect();
        var spell = CreateSpell("fire-bolt", SpellType.Targeted, MagicSchool.Fire);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.DamageDealt.Count);
        Assert.AreEqual("Fire", result.DamageDealt[0].DamageType);
        Assert.AreEqual(5, result.DamageDealt[0].FatigueDamage); // SV 3 = 5 FAT
        Assert.AreEqual(1, result.DamageDealt[0].VitalityDamage); // SV 3 = 1 VIT
    }

    [TestMethod]
    public void EnergyDamageSpellEffect_IceShard_ColdDamage()
    {
        var effect = new EnergyDamageSpellEffect();
        var spell = CreateSpell("ice-shard", SpellType.Targeted, MagicSchool.Water);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Cold", result.DamageDealt[0].DamageType);
    }

    [TestMethod]
    public void EnergyDamageSpellEffect_HighSV_CausesWound()
    {
        var effect = new EnergyDamageSpellEffect();
        var spell = CreateSpell("fire-bolt", SpellType.Targeted, MagicSchool.Fire);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 7,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.DamageDealt[0].CausedWound);
    }

    [TestMethod]
    public void EnergyDamageSpellEffect_WithManaPump_IncreasesEffectiveSV()
    {
        var effect = new EnergyDamageSpellEffect();
        var spell = CreateSpell("fire-bolt", SpellType.Targeted, MagicSchool.Fire);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2,
            PumpedMana = new Dictionary<MagicSchool, int> { { MagicSchool.Fire, 2 } }
        };

        var result = effect.Resolve(context);

        // SV 2 + 2 mana pump = effective SV 4
        Assert.AreEqual(6, result.DamageDealt[0].FatigueDamage); // SV 4 = 6 FAT
    }

    #endregion

    #region HealingSpellEffect Tests

    [TestMethod]
    public void HealingSpellEffect_MinorHeal_RestoresFatigue()
    {
        var effect = new HealingSpellEffect();
        var spell = CreateSpell("minor-heal", SpellType.Targeted, MagicSchool.Life);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.HealingApplied.Count);
        Assert.AreEqual(5, result.HealingApplied[0].FatigueHealed); // SV 3 = 5 FAT
        Assert.AreEqual(0, result.HealingApplied[0].VitalityHealed);
    }

    [TestMethod]
    public void HealingSpellEffect_RestoreVitality_RestoresVIT()
    {
        var effect = new HealingSpellEffect();
        var spell = CreateSpell("restore-vitality", SpellType.Targeted, MagicSchool.Life);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.HealingApplied[0].FatigueHealed);
        Assert.AreEqual(2, result.HealingApplied[0].VitalityHealed); // SV 3 = 2 VIT
    }

    [TestMethod]
    public void HealingSpellEffect_SelfTargetWithRange0_TargetsCaster()
    {
        var effect = new HealingSpellEffect();
        var spell = CreateSpell("minor-heal", SpellType.Targeted, MagicSchool.Life);
        spell.Range = 0; // Self-targeting
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1
            // No TargetCharacterId - should use caster
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.HealingApplied[0].CharacterId); // Healed caster
    }

    [TestMethod]
    public void HealingSpellEffect_NegativeSV_StillProvidesMinimalHealing()
    {
        var effect = new HealingSpellEffect();
        var spell = CreateSpell("minor-heal", SpellType.Targeted, MagicSchool.Life);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = -2,
            CasterId = 1,
            TargetCharacterId = 2
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.HealingApplied[0].FatigueHealed); // Minimal healing
    }

    [TestMethod]
    public void HealingSpellEffect_WithPump_IncreasesHealing()
    {
        var effect = new HealingSpellEffect();
        var spell = CreateSpell("minor-heal", SpellType.Targeted, MagicSchool.Life);
        
        var contextNoPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2
        };
        
        var contextWithPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetCharacterId = 2,
            PumpedFatigue = 2,
            PumpedMana = new Dictionary<MagicSchool, int> { { MagicSchool.Life, 1 } }
        };

        var resultNoPump = effect.Resolve(contextNoPump);
        var resultWithPump = effect.Resolve(contextWithPump);

        // SV 2 vs SV 2 + 3 pump = SV 5
        Assert.IsTrue(resultWithPump.HealingApplied[0].FatigueHealed > resultNoPump.HealingApplied[0].FatigueHealed);
    }

    #endregion

    #region AreaLightSpellEffect Tests

    [TestMethod]
    public void AreaLightSpellEffect_CreatesLocationAndEffect()
    {
        var effect = new AreaLightSpellEffect();
        var spell = CreateSpell("illuminate-area", SpellType.AreaEffect, MagicSchool.Light);
        spell.DefaultDuration = 60;
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1,
            TargetLocation = "Town Square",
            CampaignId = 1
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.AffectedLocation);
        Assert.AreEqual("Town Square", result.AffectedLocation.Name);
        Assert.IsNotNull(result.CreatedLocationEffect);
        Assert.IsTrue(result.CreatedLocationEffect.RoundsRemaining > 60); // Base + SV bonus
    }

    [TestMethod]
    public void AreaLightSpellEffect_WithPump_ExtendsDuration()
    {
        var effect = new AreaLightSpellEffect();
        var spell = CreateSpell("illuminate-area", SpellType.AreaEffect, MagicSchool.Light);
        spell.DefaultDuration = 60;
        
        var contextNoPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 0,
            CasterId = 1,
            TargetLocation = "Cave Entrance",
            CampaignId = 1
        };
        
        var contextWithPump = new SpellEffectContext
        {
            Spell = spell,
            SV = 0,
            CasterId = 1,
            TargetLocation = "Cave Entrance",
            CampaignId = 1,
            PumpedFatigue = 3
        };

        var resultNoPump = effect.Resolve(contextNoPump);
        var resultWithPump = effect.Resolve(contextWithPump);

        // Pump adds 20 rounds per point
        Assert.IsTrue(resultWithPump.CreatedLocationEffect!.RoundsRemaining > 
                     resultNoPump.CreatedLocationEffect!.RoundsRemaining);
    }

    [TestMethod]
    public void AreaLightSpellEffect_NoLocation_Fails()
    {
        var effect = new AreaLightSpellEffect();
        var spell = CreateSpell("illuminate-area", SpellType.AreaEffect, MagicSchool.Light);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1
            // No TargetLocation
        };

        var result = effect.Resolve(context);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
    }

    #endregion

    #region WallOfFireSpellEffect Tests

    [TestMethod]
    public void WallOfFireSpellEffect_CreatesLocationEffect()
    {
        var effect = new WallOfFireSpellEffect();
        var spell = CreateSpell("wall-of-fire", SpellType.Environmental, MagicSchool.Fire);
        spell.DefaultDuration = 10;
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 4,
            CasterId = 1,
            TargetLocation = "Dungeon Corridor",
            CampaignId = 1
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.AffectedLocation);
        Assert.IsNotNull(result.CreatedLocationEffect);
        Assert.AreEqual("wall-of-fire", result.CreatedLocationEffect.SpellSkillId);
    }

    [TestMethod]
    public void WallOfFireSpellEffect_DamagesTargetsInArea()
    {
        var effect = new WallOfFireSpellEffect();
        var spell = CreateSpell("wall-of-fire", SpellType.Environmental, MagicSchool.Fire);
        spell.DefaultDuration = 10;
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1,
            TargetLocation = "Dungeon Corridor",
            CampaignId = 1,
            TargetCharacterIds = [5, 6, 7]
        };

        var result = effect.Resolve(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.DamageDealt.Count);
        Assert.AreEqual(3, result.TargetResults.Count);
        Assert.IsTrue(result.DamageDealt.TrueForAll(d => d.DamageType == "Fire"));
    }

    [TestMethod]
    public void WallOfFireSpellEffect_PerTargetSV_UsesIndividualResistance()
    {
        var effect = new WallOfFireSpellEffect();
        var spell = CreateSpell("wall-of-fire", SpellType.Environmental, MagicSchool.Fire);
        spell.DefaultDuration = 10;
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 5,
            CasterId = 1,
            TargetLocation = "Dungeon Corridor",
            CampaignId = 1,
            TargetCharacterIds = [5, 6],
            TargetSVs = new Dictionary<int, int>
            {
                { 5, 2 },  // This target resisted well
                { 6, 6 }   // This target failed resistance
            }
        };

        var result = effect.Resolve(context);

        // Target 5 with SV 2: less damage
        // Target 6 with SV 6: more damage, wound
        var target5Damage = result.DamageDealt.Find(d => d.CharacterId == 5);
        var target6Damage = result.DamageDealt.Find(d => d.CharacterId == 6);

        Assert.IsNotNull(target5Damage);
        Assert.IsNotNull(target6Damage);
        Assert.IsTrue(target6Damage.FatigueDamage > target5Damage.FatigueDamage);
    }

    [TestMethod]
    public void WallOfFireSpellEffect_WithPump_IncreasesEffectiveSV()
    {
        var effect = new WallOfFireSpellEffect();
        var spell = CreateSpell("wall-of-fire", SpellType.Environmental, MagicSchool.Fire);
        spell.DefaultDuration = 10;
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 2,
            CasterId = 1,
            TargetLocation = "Dungeon Corridor",
            CampaignId = 1,
            PumpedFatigue = 2,
            TargetCharacterIds = [5]
        };

        var result = effect.Resolve(context);

        // CastSV stored should be SV + pump = 4
        Assert.AreEqual(4, result.CreatedLocationEffect!.CastSV);
    }

    [TestMethod]
    public void WallOfFireSpellEffect_CalculateRoundDamage_UsesStoredSV()
    {
        // Simulate a character entering the wall on a subsequent round
        var damage = WallOfFireSpellEffect.CalculateRoundDamage(castSV: 5, characterDefenseBonus: 2);

        // Effective SV = 5 - 2 = 3
        Assert.AreEqual(5, damage.FatigueDamage); // SV 3 = 5 FAT
        Assert.AreEqual(1, damage.VitalityDamage); // SV 3 = 1 VIT
    }

    [TestMethod]
    public void WallOfFireSpellEffect_NoLocation_Fails()
    {
        var effect = new WallOfFireSpellEffect();
        var spell = CreateSpell("wall-of-fire", SpellType.Environmental, MagicSchool.Fire);
        var context = new SpellEffectContext
        {
            Spell = spell,
            SV = 3,
            CasterId = 1
            // No TargetLocation
        };

        var result = effect.Resolve(context);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
    }

    #endregion

    #region EnergyDamage Table Tests

    [TestMethod]
    public void EnergyDamageTable_NegativeSV_NoDamage()
    {
        var result = EnergyDamageSpellEffect.GetEnergyDamage(-3);

        Assert.AreEqual(0, result.FatigueDamage);
        Assert.AreEqual(0, result.VitalityDamage);
        Assert.IsFalse(result.CausesWound);
    }

    [TestMethod]
    public void EnergyDamageTable_SV0_Graze()
    {
        var result = EnergyDamageSpellEffect.GetEnergyDamage(0);

        Assert.AreEqual(2, result.FatigueDamage);
        Assert.AreEqual(0, result.VitalityDamage);
    }

    [TestMethod]
    public void EnergyDamageTable_SV6_CausesWound()
    {
        var result = EnergyDamageSpellEffect.GetEnergyDamage(6);

        Assert.AreEqual(8, result.FatigueDamage);
        Assert.AreEqual(3, result.VitalityDamage);
        Assert.IsTrue(result.CausesWound);
    }

    [TestMethod]
    public void EnergyDamageTable_HighSV_ScalesUp()
    {
        var sv8 = EnergyDamageSpellEffect.GetEnergyDamage(8);
        var sv10 = EnergyDamageSpellEffect.GetEnergyDamage(10);

        Assert.IsTrue(sv10.FatigueDamage > sv8.FatigueDamage);
        Assert.IsTrue(sv10.VitalityDamage > sv8.VitalityDamage);
    }

    #endregion

    #region Healing Table Tests

    [TestMethod]
    public void HealingTable_FatigueHealing_SV3()
    {
        var result = HealingSpellEffect.GetHealing(3, HealingType.Fatigue);

        Assert.AreEqual(5, result.FatigueHealed);
        Assert.AreEqual(0, result.VitalityHealed);
    }

    [TestMethod]
    public void HealingTable_VitalityHealing_SV4()
    {
        var result = HealingSpellEffect.GetHealing(4, HealingType.Vitality);

        Assert.AreEqual(0, result.FatigueHealed);
        Assert.AreEqual(3, result.VitalityHealed);
    }

    [TestMethod]
    public void HealingTable_BothHealing_SV5()
    {
        var result = HealingSpellEffect.GetHealing(5, HealingType.Both);

        Assert.AreEqual(7, result.FatigueHealed);
        Assert.AreEqual(3, result.VitalityHealed);
    }

    #endregion

    #region Helper Methods

    private static SpellDefinition CreateSpell(
        string skillId, 
        SpellType spellType, 
        MagicSchool school = MagicSchool.Life)
    {
        return new SpellDefinition
        {
            SkillId = skillId,
            SpellType = spellType,
            MagicSchool = school,
            ManaCost = 1,
            Range = 2,
            ResistanceType = SpellResistanceType.None
        };
    }

    #endregion
}
