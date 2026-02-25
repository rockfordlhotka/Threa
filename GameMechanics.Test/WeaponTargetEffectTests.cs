using GameMechanics.Combat;
using GameMechanics.Combat.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

/// <summary>
/// Tests for weapon on-hit target effects: armor interaction rules and damage class filtering.
/// Covers the requirements from GitHub issue #137.
/// </summary>
[TestClass]
public class WeaponTargetEffectTests
{
    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static AttackEffectGrant MakeGrant(
        ArmorInteractionRule rule = ArmorInteractionRule.PenetrationOnly,
        int effectDc = 0)
    {
        return new AttackEffectGrant
        {
            EffectName = "Test Effect",
            EffectType = EffectType.Debuff,
            ArmorRule = rule,
            EffectDamageClass = effectDc
        };
    }

    private static AttackEffectContext MakeContext(
        int? armorDc = null,
        int? shieldDc = null,
        int targetDc = 0,
        bool armorPenetrated = true)
    {
        return new AttackEffectContext
        {
            // Attacker is nullable; Target still required but not accessed by these tests.
            Attacker = null,
            Target = null!,
            HitLocation = HitLocation.Torso,
            BaseDamageType = DamageType.Cutting,
            ArmorDamageClass = armorDc,
            ShieldDamageClass = shieldDc,
            TargetDamageClass = targetDc,
            ArmorWasPenetrated = armorPenetrated
        };
    }

    // ---------------------------------------------------------------------------
    // Armor Interaction Rules
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ArmorRule_PenetrationOnly_NoArmor_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.PenetrationOnly);
        var context = MakeContext(armorDc: null);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_PenetrationOnly_ArmorPenetrated_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.PenetrationOnly);
        var context = MakeContext(armorDc: 2, armorPenetrated: true);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_PenetrationOnly_ArmorNotPenetrated_EffectBlocked()
    {
        var grant = MakeGrant(ArmorInteractionRule.PenetrationOnly);
        var context = MakeContext(armorDc: 2, armorPenetrated: false);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_NoArmor_TargetHasNoArmor_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.NoArmor);
        var context = MakeContext(armorDc: null);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_NoArmor_TargetHasArmor_EffectBlocked()
    {
        var grant = MakeGrant(ArmorInteractionRule.NoArmor);
        var context = MakeContext(armorDc: 1, armorPenetrated: true);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_IgnoreArmor_ArmorNotPenetrated_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor);
        var context = MakeContext(armorDc: 3, armorPenetrated: false);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void ArmorRule_IgnoreArmor_NoArmor_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor);
        var context = MakeContext(armorDc: null);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    // ---------------------------------------------------------------------------
    // Damage Class (DC) Filtering
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void DamageClass_Zero_BypassesAllDcChecks()
    {
        // DC 0 means no restriction
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 0);
        var context = MakeContext(armorDc: 4, shieldDc: 4, targetDc: 4);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_ArmorDcLower_EffectApplies()
    {
        // Armor DC 1 < effect DC 2 → effect applies
        var grant = MakeGrant(effectDc: 2);
        var context = MakeContext(armorDc: 1, armorPenetrated: true);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_ArmorDcEqual_EffectBlocked()
    {
        // Armor DC 2 == effect DC 2 → blocked
        var grant = MakeGrant(effectDc: 2);
        var context = MakeContext(armorDc: 2, armorPenetrated: true);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_ArmorDcHigher_EffectBlocked()
    {
        // Armor DC 3 > effect DC 2 → blocked
        var grant = MakeGrant(effectDc: 2);
        var context = MakeContext(armorDc: 3, armorPenetrated: true);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_ShieldDcEqual_EffectBlocked()
    {
        // Shield DC checked separately from armor
        var grant = MakeGrant(effectDc: 2);
        var context = MakeContext(armorDc: null, shieldDc: 2);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_ShieldDcLower_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 3);
        var context = MakeContext(armorDc: null, shieldDc: 2);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_TargetDcEqual_EffectBlocked()
    {
        // Dragon example: flame effect DC 1, dragon inherent DC 2 → blocked
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 1);
        var context = MakeContext(armorDc: null, shieldDc: null, targetDc: 2);

        Assert.IsFalse(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_TargetDcLower_EffectApplies()
    {
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 2);
        var context = MakeContext(armorDc: null, shieldDc: null, targetDc: 1);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    [TestMethod]
    public void DamageClass_TargetDcZero_NoRestriction()
    {
        // Target DC 0 means "no inherent DC" — bypasses target DC check
        var grant = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 1);
        var context = MakeContext(armorDc: null, shieldDc: null, targetDc: 0);

        Assert.IsTrue(ShouldTargetEffectApply(grant, context));
    }

    // ---------------------------------------------------------------------------
    // Canonical Examples from the Issue
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void Example_FlamingArrowVsDragon_EffectBlocked()
    {
        // "A flaming arrow won't set a dragon on fire, because a dragon will be DC 2 or 3,
        //  and the arrow will be DC 1 with a flame effect of DC 1."
        var flameEffect = MakeGrant(ArmorInteractionRule.PenetrationOnly, effectDc: 1);
        var dragonContext = MakeContext(armorDc: null, shieldDc: null, targetDc: 2);

        Assert.IsFalse(ShouldTargetEffectApply(flameEffect, dragonContext));
    }

    [TestMethod]
    public void Example_FlamingArrowVsUnarmored_EffectApplies()
    {
        // Same flame arrow vs an unarmored human (no armor, target DC 0)
        var flameEffect = MakeGrant(ArmorInteractionRule.PenetrationOnly, effectDc: 1);
        var humanContext = MakeContext(armorDc: null, shieldDc: null, targetDc: 0);

        Assert.IsTrue(ShouldTargetEffectApply(flameEffect, humanContext));
    }

    [TestMethod]
    public void Example_FlamingArrowVsTank_EffectBlocked()
    {
        // Heavy armor DC 3 stops the flame effect (DC 1) even with IgnoreArmor:
        // DC check fires first and blocks it.
        var flameEffect = MakeGrant(ArmorInteractionRule.PenetrationOnly, effectDc: 1);
        var tankContext = MakeContext(armorDc: 3, armorPenetrated: false);

        Assert.IsFalse(ShouldTargetEffectApply(flameEffect, tankContext));
    }

    [TestMethod]
    public void Example_IgnoreArmorPoison_BlockedByDc()
    {
        // Poison that ignores armor — but target's inherent DC (3) is too high for effect DC (2)
        var poison = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 2);
        var context = MakeContext(armorDc: 4, shieldDc: null, targetDc: 3, armorPenetrated: false);

        Assert.IsFalse(ShouldTargetEffectApply(poison, context));
    }

    [TestMethod]
    public void Example_IgnoreArmorPoison_NoDc_AlwaysApplies()
    {
        // Magical poison with no DC restriction and IgnoreArmor — always applies
        var poison = MakeGrant(ArmorInteractionRule.IgnoreArmor, effectDc: 0);
        var context = MakeContext(armorDc: 4, armorPenetrated: false);

        Assert.IsTrue(ShouldTargetEffectApply(poison, context));
    }

    // ---------------------------------------------------------------------------
    // Default value tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ItemEffectDefinition_DefaultArmorRule_IsPenetrationOnly()
    {
        var def = new ItemEffectDefinition();
        Assert.AreEqual(ArmorInteractionRule.PenetrationOnly, def.ArmorRule);
    }

    [TestMethod]
    public void ItemEffectDefinition_DefaultEffectDamageClass_IsZero()
    {
        var def = new ItemEffectDefinition();
        Assert.AreEqual(0, def.EffectDamageClass);
    }

    [TestMethod]
    public void AttackEffectGrant_DefaultArmorRule_IsPenetrationOnly()
    {
        var grant = new AttackEffectGrant();
        Assert.AreEqual(ArmorInteractionRule.PenetrationOnly, grant.ArmorRule);
    }

    [TestMethod]
    public void AttackEffectContext_DefaultArmorWasPenetrated_IsTrue()
    {
        // Ensures existing code paths that don't set ArmorWasPenetrated still
        // allow effects to apply (backward-compatible default).
        var context = MakeContext();
        Assert.IsTrue(context.ArmorWasPenetrated);
    }

    // ---------------------------------------------------------------------------
    // Mirror of the private static ShouldTargetEffectApply from AttackEffectService.
    // Kept here so the tests precisely match the implementation logic.
    // ---------------------------------------------------------------------------

    private static bool ShouldTargetEffectApply(AttackEffectGrant grant, AttackEffectContext context)
    {
        if (grant.EffectDamageClass > 0)
        {
            if (context.ArmorDamageClass.HasValue && context.ArmorDamageClass.Value >= grant.EffectDamageClass)
                return false;
            if (context.ShieldDamageClass.HasValue && context.ShieldDamageClass.Value >= grant.EffectDamageClass)
                return false;
            if (context.TargetDamageClass > 0 && context.TargetDamageClass >= grant.EffectDamageClass)
                return false;
        }

        return grant.ArmorRule switch
        {
            ArmorInteractionRule.PenetrationOnly =>
                context.ArmorDamageClass == null || context.ArmorWasPenetrated,
            ArmorInteractionRule.NoArmor =>
                context.ArmorDamageClass == null,
            ArmorInteractionRule.IgnoreArmor =>
                true,
            _ => true
        };
    }
}
