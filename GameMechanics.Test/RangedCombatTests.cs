using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMechanics.Combat;

namespace GameMechanics.Test
{
  [TestClass]
  public class RangedCombatTests
  {
    #region RangeModifiers Tests

    [TestMethod]
    [DataRow(RangeCategory.Short, 6)]
    [DataRow(RangeCategory.Medium, 8)]
    [DataRow(RangeCategory.Long, 10)]
    [DataRow(RangeCategory.Extreme, 12)]
    public void GetBaseTV_ReturnsCorrectTV_ForEachCategory(RangeCategory category, int expectedTV)
    {
      Assert.AreEqual(expectedTV, RangeModifiers.GetBaseTV(category));
    }

    [TestMethod]
    public void GetBaseTV_OutOfRange_ReturnsMaxValue()
    {
      Assert.AreEqual(int.MaxValue, RangeModifiers.GetBaseTV(RangeCategory.OutOfRange));
    }

    [TestMethod]
    public void GetTargetMovementModifier_Moving_Returns2()
    {
      Assert.AreEqual(2, RangeModifiers.GetTargetMovementModifier(isMoving: true, isProne: false, isCrouching: false));
    }

    [TestMethod]
    public void GetTargetMovementModifier_Prone_Returns2()
    {
      Assert.AreEqual(2, RangeModifiers.GetTargetMovementModifier(isMoving: false, isProne: true, isCrouching: false));
    }

    [TestMethod]
    public void GetTargetMovementModifier_Crouching_Returns2()
    {
      Assert.AreEqual(2, RangeModifiers.GetTargetMovementModifier(isMoving: false, isProne: false, isCrouching: true));
    }

    [TestMethod]
    public void GetTargetMovementModifier_Multiple_Stacks()
    {
      Assert.AreEqual(4, RangeModifiers.GetTargetMovementModifier(isMoving: true, isProne: true, isCrouching: false));
      Assert.AreEqual(6, RangeModifiers.GetTargetMovementModifier(isMoving: true, isProne: true, isCrouching: true));
    }

    [TestMethod]
    [DataRow(TargetSize.Normal, 0)]
    [DataRow(TargetSize.Small, 1)]
    [DataRow(TargetSize.Tiny, 2)]
    public void GetSizeModifier_ReturnsCorrectValue(TargetSize size, int expected)
    {
      Assert.AreEqual(expected, RangeModifiers.GetSizeModifier(size));
    }

    [TestMethod]
    [DataRow(CoverType.None, 0)]
    [DataRow(CoverType.Half, 1)]
    [DataRow(CoverType.ThreeQuarters, 2)]
    public void GetCoverModifier_ReturnsCorrectValue(CoverType cover, int expected)
    {
      Assert.AreEqual(expected, RangeModifiers.GetCoverModifier(cover));
    }

    [TestMethod]
    public void GetAttackerModifier_Moving_Returns2()
    {
      Assert.AreEqual(2, RangeModifiers.GetAttackerModifier(isMoving: true));
      Assert.AreEqual(0, RangeModifiers.GetAttackerModifier(isMoving: false));
    }

    [TestMethod]
    public void CalculateTotalModifier_Stacks_AllModifiers()
    {
      // Moving target (+2) + Small target (+1) + Half cover (+1) + Moving attacker (+2) = +6
      int total = RangeModifiers.CalculateTotalModifier(
        targetMoving: true,
        targetSize: TargetSize.Small,
        cover: CoverType.Half,
        attackerMoving: true);

      Assert.AreEqual(6, total);
    }

    [TestMethod]
    public void DetermineRangeCategory_ShortRange_ReturnsShort()
    {
      var category = RangeModifiers.DetermineRangeCategory(
        distanceRangeValue: 2,
        shortRange: 3, mediumRange: 5, longRange: 7, extremeRange: 9);

      Assert.AreEqual(RangeCategory.Short, category);
    }

    [TestMethod]
    public void DetermineRangeCategory_MediumRange_ReturnsMedium()
    {
      var category = RangeModifiers.DetermineRangeCategory(
        distanceRangeValue: 4,
        shortRange: 3, mediumRange: 5, longRange: 7, extremeRange: 9);

      Assert.AreEqual(RangeCategory.Medium, category);
    }

    [TestMethod]
    public void DetermineRangeCategory_LongRange_ReturnsLong()
    {
      var category = RangeModifiers.DetermineRangeCategory(
        distanceRangeValue: 6,
        shortRange: 3, mediumRange: 5, longRange: 7, extremeRange: 9);

      Assert.AreEqual(RangeCategory.Long, category);
    }

    [TestMethod]
    public void DetermineRangeCategory_ExtremeRange_ReturnsExtreme()
    {
      var category = RangeModifiers.DetermineRangeCategory(
        distanceRangeValue: 8,
        shortRange: 3, mediumRange: 5, longRange: 7, extremeRange: 9);

      Assert.AreEqual(RangeCategory.Extreme, category);
    }

    [TestMethod]
    public void DetermineRangeCategory_BeyondExtreme_ReturnsOutOfRange()
    {
      var category = RangeModifiers.DetermineRangeCategory(
        distanceRangeValue: 10,
        shortRange: 3, mediumRange: 5, longRange: 7, extremeRange: 9);

      Assert.AreEqual(RangeCategory.OutOfRange, category);
    }

    #endregion

    #region WeaponRanges Tests

    [TestMethod]
    public void WeaponRanges_Shortbow_HasCorrectRanges()
    {
      var ranges = WeaponRanges.Shortbow;
      Assert.AreEqual(3, ranges.Short);
      Assert.AreEqual(5, ranges.Medium);
      Assert.AreEqual(7, ranges.Long);
      Assert.AreEqual(9, ranges.Extreme);
    }

    [TestMethod]
    public void WeaponRanges_Longbow_HasCorrectRanges()
    {
      var ranges = WeaponRanges.Longbow;
      Assert.AreEqual(4, ranges.Short);
      Assert.AreEqual(6, ranges.Medium);
      Assert.AreEqual(8, ranges.Long);
      Assert.AreEqual(10, ranges.Extreme);
    }

    [TestMethod]
    public void WeaponRanges_Crossbow_HasCorrectRanges()
    {
      var ranges = WeaponRanges.Crossbow;
      Assert.AreEqual(5, ranges.Short);
      Assert.AreEqual(7, ranges.Medium);
      Assert.AreEqual(9, ranges.Long);
      Assert.AreEqual(11, ranges.Extreme);
    }

    [TestMethod]
    public void WeaponRanges_ThrownDagger_HasCorrectRanges()
    {
      var ranges = WeaponRanges.ThrownDagger;
      Assert.AreEqual(1, ranges.Short);
      Assert.AreEqual(2, ranges.Medium);
      Assert.AreEqual(3, ranges.Long);
      Assert.AreEqual(4, ranges.Extreme);
    }

    [TestMethod]
    public void WeaponRanges_GetCategory_ReturnsCorrectCategory()
    {
      var ranges = WeaponRanges.Shortbow;

      Assert.AreEqual(RangeCategory.Short, ranges.GetCategory(3));
      Assert.AreEqual(RangeCategory.Medium, ranges.GetCategory(5));
      Assert.AreEqual(RangeCategory.Long, ranges.GetCategory(7));
      Assert.AreEqual(RangeCategory.Extreme, ranges.GetCategory(9));
      Assert.AreEqual(RangeCategory.OutOfRange, ranges.GetCategory(10));
    }

    #endregion

    #region RangedAttackRequest Tests

    [TestMethod]
    public void RangedAttackRequest_GetFinalTV_IncludesAllModifiers()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3, // Short range = TV 6
        TargetMoving = true,    // +2
        Cover = CoverType.Half  // +1
      };

      // Base TV 6 + modifiers 3 = 9
      Assert.AreEqual(9, request.GetFinalTV());
    }

    [TestMethod]
    public void RangedAttackRequest_GetEffectiveAS_AppliesMultipleActionPenalty()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        ActionsThisRound = 1
      };

      Assert.AreEqual(11, request.GetEffectiveAS()); // 12 - 1
    }

    [TestMethod]
    public void RangedAttackRequest_GetEffectiveAS_AppliesBoosts()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        APBoost = 2,
        FATBoost = 1
      };

      Assert.AreEqual(15, request.GetEffectiveAS()); // 12 + 2 + 1
    }

    [TestMethod]
    public void RangedAttackRequest_GetEffectiveAS_AppliesAimBonus()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        HasAimBonus = true
      };

      Assert.AreEqual(14, request.GetEffectiveAS()); // 12 + 2
    }

    [TestMethod]
    public void RangedAttackRequest_GetEffectiveAS_CombinesAllModifiers()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        ActionsThisRound = 1,  // -1
        APBoost = 2,           // +2
        FATBoost = 1,          // +1
        HasAimBonus = true     // +2
      };

      Assert.AreEqual(16, request.GetEffectiveAS()); // 12 - 1 + 2 + 1 + 2
    }

    [TestMethod]
    public void RangedAttackRequest_IsOutOfRange_DetectsOutOfRange()
    {
      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 10 // Beyond extreme (9)
      };

      Assert.IsTrue(request.IsOutOfRange);
    }

    #endregion

    #region RangedAttackResolver Tests

    [TestMethod]
    public void Resolve_OutOfRange_ReturnsOutOfRangeResult()
    {
      var roller = new DeterministicDiceRoller();
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 15,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 10 // Out of range
      };

      var result = resolver.Resolve(request);

      Assert.IsFalse(result.IsHit);
      Assert.IsTrue(result.WasOutOfRange);
      Assert.AreEqual(RangeCategory.OutOfRange, result.RangeCategory);
    }

    [TestMethod]
    public void Resolve_ShortRange_UsesTV6()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5); // Good roll to ensure hit
      roller.QueueDiceRolls(10); // Location roll
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3 // Short range
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(6, result.BaseTV);
      Assert.AreEqual(RangeCategory.Short, result.RangeCategory);
    }

    [TestMethod]
    public void Resolve_MediumRange_UsesTV8()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5);
      roller.QueueDiceRolls(10);
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 4 // Medium range
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(8, result.BaseTV);
      Assert.AreEqual(RangeCategory.Medium, result.RangeCategory);
    }

    [TestMethod]
    public void Resolve_LongRange_UsesTV10()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5);
      roller.QueueDiceRolls(10);
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 6 // Long range
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(10, result.BaseTV);
      Assert.AreEqual(RangeCategory.Long, result.RangeCategory);
    }

    [TestMethod]
    public void Resolve_ExtremeRange_UsesTV12()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5);
      roller.QueueDiceRolls(10);
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 8 // Extreme range
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(12, result.BaseTV);
      Assert.AreEqual(RangeCategory.Extreme, result.RangeCategory);
    }

    [TestMethod]
    public void Resolve_Miss_ReturnsNegativeSV()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(-3); // Low roll
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 8, // AS 8 + roll -3 = AV 5 vs TV 6 = SV -1
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3
      };

      var result = resolver.Resolve(request);

      Assert.IsFalse(result.IsHit);
      Assert.AreEqual(-1, result.SV);
    }

    [TestMethod]
    public void Resolve_Hit_ReturnsHitLocation()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5); // Attack roll
      roller.QueueDiceRolls(12); // Hit location roll
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10, // AS 10 + roll 5 = AV 15 vs TV 6 = SV 9
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3
      };

      var result = resolver.Resolve(request);

      Assert.IsTrue(result.IsHit);
      Assert.IsNotNull(result.HitLocation);
      Assert.AreEqual(HitLocation.Torso, result.HitLocation.Value); // Roll 12 = Torso
    }

    [TestMethod]
    public void Resolve_RegularRanged_NoBonusPhysicality()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(3); // Attack
      roller.QueueDiceRolls(10); // Location
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        IsThrownWeapon = false
      };

      var result = resolver.Resolve(request);

      Assert.IsTrue(result.IsHit);
      Assert.IsFalse(result.IsThrownWeapon);
      Assert.IsNull(result.PhysicalityBonus);
      Assert.AreEqual(result.SV, result.FinalSV);
    }

    [TestMethod]
    public void Resolve_ThrownWeapon_IncludesPhysicalityBonus()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(3, 5); // Attack, physicality
      roller.QueueDiceRolls(10); // Location
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        AttackerPhysicalityAS = 12, // Phys 12 + roll 5 = 17 vs TV 8 = RV 9
        WeaponRanges = WeaponRanges.ThrownDagger,
        DistanceRangeValue = 1,
        IsThrownWeapon = true
      };

      var result = resolver.Resolve(request);

      Assert.IsTrue(result.IsHit);
      Assert.IsTrue(result.IsThrownWeapon);
      Assert.IsNotNull(result.PhysicalityBonus);
      Assert.IsNotNull(result.PhysicalityRV);
      Assert.AreEqual(9, result.PhysicalityRV.Value); // 17 - 8
      Assert.IsTrue(result.FinalSV > result.SV); // Should have positive modifier
    }

    [TestMethod]
    public void Resolve_WithModifiers_IncreaseTV()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(0); // Neutral roll
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3, // Short = TV 6
        TargetMoving = true,     // +2
        Cover = CoverType.Half   // +1
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(6, result.BaseTV);
      Assert.AreEqual(3, result.TVModifiers);
      Assert.AreEqual(9, result.TV);
    }

    [TestMethod]
    public void Resolve_WithAimBonus_AppliesAS2()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(0); // Attack
      roller.QueueDiceRolls(10); // Location
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        HasAimBonus = true
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(12, result.EffectiveAS); // 10 + 2 aim bonus
      Assert.IsTrue(result.IsHit);
    }

    [TestMethod]
    public void Resolve_MultipleActionPenalty_AppliesOnce()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(0);
      roller.QueueDiceRolls(10);
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        ActionsThisRound = 2 // Multiple actions but still only -1
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(9, result.EffectiveAS); // 10 - 1
    }

    [TestMethod]
    public void Resolve_CalculatesDamage()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(5); // Good roll = high SV
      roller.QueueDiceRolls(10); // Location
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3
      };

      var result = resolver.Resolve(request);

      Assert.IsTrue(result.IsHit);
      Assert.IsNotNull(result.Damage);
      Assert.IsTrue(result.Damage.VitalityDamage > 0 || result.Damage.FatigueDamage > 0);
    }

    #endregion

    #region AimState Tests

    [TestMethod]
    public void AimState_StartAiming_SetsTarget()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      Assert.IsTrue(aimState.IsAiming);
      Assert.AreEqual("target1", aimState.TargetId);
      Assert.AreEqual(5, aimState.AimRound);
    }

    [TestMethod]
    public void AimState_CheckAimBonus_TrueWhenConditionsMet()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      bool hasBonus = aimState.CheckAimBonus(
        targetId: "target1",
        currentRound: 6,    // Next round
        actionsThisRound: 0 // First action
      );

      Assert.IsTrue(hasBonus);
    }

    [TestMethod]
    public void AimState_CheckAimBonus_FalseWhenNotAiming()
    {
      var aimState = new AimState();

      bool hasBonus = aimState.CheckAimBonus("target1", 6, 0);

      Assert.IsFalse(hasBonus);
    }

    [TestMethod]
    public void AimState_CheckAimBonus_FalseWhenWrongRound()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      // Same round - no bonus
      Assert.IsFalse(aimState.CheckAimBonus("target1", 5, 0));

      // Two rounds later - no bonus
      Assert.IsFalse(aimState.CheckAimBonus("target1", 7, 0));
    }

    [TestMethod]
    public void AimState_CheckAimBonus_FalseWhenNotFirstAction()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      bool hasBonus = aimState.CheckAimBonus("target1", 6, actionsThisRound: 1);

      Assert.IsFalse(hasBonus);
    }

    [TestMethod]
    public void AimState_CheckAimBonus_FalseWhenDifferentTarget()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      bool hasBonus = aimState.CheckAimBonus("target2", 6, 0);

      Assert.IsFalse(hasBonus);
    }

    [TestMethod]
    public void AimState_ConsumeAim_ClearsState()
    {
      var aimState = new AimState();
      aimState.StartAiming("target1", currentRound: 5);

      aimState.ConsumeAim();

      Assert.IsFalse(aimState.IsAiming);
      Assert.IsNull(aimState.TargetId);
    }

    #endregion

    #region PrepState Tests

    [TestMethod]
    public void PrepState_PrepItem_AddsItem()
    {
      var prepState = new PrepState();

      int count = prepState.PrepItem("Arrow");

      Assert.AreEqual(1, count);
      Assert.AreEqual(1, prepState.GetPreppedCount("Arrow"));
    }

    [TestMethod]
    public void PrepState_PrepItem_StacksMultiple()
    {
      var prepState = new PrepState();

      prepState.PrepItem("Arrow");
      prepState.PrepItem("Arrow");
      prepState.PrepItem("Arrow");

      Assert.AreEqual(3, prepState.GetPreppedCount("Arrow"));
    }

    [TestMethod]
    public void PrepState_UsePreppedItem_ConsumesOne()
    {
      var prepState = new PrepState();
      prepState.PrepItem("Arrow");
      prepState.PrepItem("Arrow");

      bool used = prepState.UsePreppedItem("Arrow");

      Assert.IsTrue(used);
      Assert.AreEqual(1, prepState.GetPreppedCount("Arrow"));
    }

    [TestMethod]
    public void PrepState_UsePreppedItem_ReturnsFalseWhenNone()
    {
      var prepState = new PrepState();

      bool used = prepState.UsePreppedItem("Arrow");

      Assert.IsFalse(used);
    }

    [TestMethod]
    public void PrepState_HasPreppedItem_ReturnsCorrectly()
    {
      var prepState = new PrepState();
      prepState.PrepItem("Arrow");

      Assert.IsTrue(prepState.HasPreppedItem("Arrow"));
      Assert.IsFalse(prepState.HasPreppedItem("Bolt"));
    }

    [TestMethod]
    public void PrepState_ClearAll_RemovesAll()
    {
      var prepState = new PrepState();
      prepState.PrepItem("Arrow");
      prepState.PrepItem("Bolt");

      prepState.ClearAll();

      Assert.AreEqual(0, prepState.GetPreppedCount("Arrow"));
      Assert.AreEqual(0, prepState.GetPreppedCount("Bolt"));
    }

    #endregion

    #region RangedCooldowns Tests

    [TestMethod]
    [DataRow(0, 6.0)]
    [DataRow(1, 5.0)]
    [DataRow(2, 4.0)]
    [DataRow(3, 3.0)]
    [DataRow(4, 2.0)]
    [DataRow(5, 2.0)]
    [DataRow(6, 1.0)]
    [DataRow(7, 1.0)]
    [DataRow(8, 0.5)]
    [DataRow(9, 0.5)]
    [DataRow(10, 0.0)]
    [DataRow(15, 0.0)]
    public void GetCooldownSeconds_ReturnsCorrectDuration(int skillLevel, double expectedSeconds)
    {
      Assert.AreEqual(expectedSeconds, RangedCooldowns.GetCooldownSeconds(skillLevel));
    }

    [TestMethod]
    public void GetInterruptionBehavior_ThrownWeapon_ReturnsResettable()
    {
      var behavior = RangedCooldowns.GetInterruptionBehavior(isThrownWeapon: true);
      Assert.AreEqual(Time.CooldownBehavior.Resettable, behavior);
    }

    [TestMethod]
    public void GetInterruptionBehavior_BowCrossbow_ReturnsPausable()
    {
      var behavior = RangedCooldowns.GetInterruptionBehavior(isThrownWeapon: false);
      Assert.AreEqual(Time.CooldownBehavior.Pausable, behavior);
    }

    [TestMethod]
    public void CanFireWithPreppedAmmo_ReturnsTrueWhenPrepped()
    {
      var prepState = new PrepState();
      prepState.PrepItem("Arrow");

      Assert.IsTrue(RangedCooldowns.CanFireWithPreppedAmmo(prepState, "Arrow"));
    }

    [TestMethod]
    public void CanFireWithPreppedAmmo_ReturnsFalseWhenNotPrepped()
    {
      var prepState = new PrepState();

      Assert.IsFalse(RangedCooldowns.CanFireWithPreppedAmmo(prepState, "Arrow"));
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void FullRangedAttackFlow_Shortbow_AtShortRange()
    {
      // Setup: Archer with Bow skill 12, shooting at short range
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(2); // Attack roll +2
      roller.QueueDiceRolls(13); // Hit location 13 (left arm per map: 13-14)
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 12,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3, // 9 meters, short range
        IsThrownWeapon = false
      };

      var result = resolver.Resolve(request);

      // AV = 12 + 2 = 14
      // TV = 6 (short range)
      // SV = 14 - 6 = 8
      Assert.IsTrue(result.IsHit);
      Assert.AreEqual(8, result.SV);
      Assert.AreEqual(HitLocation.LeftArm, result.HitLocation);
    }

    [TestMethod]
    public void FullRangedAttackFlow_ThrownDagger_WithPhysicality()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(1, 3); // Attack +1, phys +3
      roller.QueueDiceRolls(10); // Location 10 (torso)
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,        // Melee skill
        AttackerPhysicalityAS = 14, // Strong character
        WeaponRanges = WeaponRanges.ThrownDagger,
        DistanceRangeValue = 1, // 1 meter, point blank
        IsThrownWeapon = true
      };

      var result = resolver.Resolve(request);

      Assert.IsTrue(result.IsHit);
      Assert.IsTrue(result.IsThrownWeapon);
      Assert.IsNotNull(result.PhysicalityBonus);
      // Phys: 14 + 3 = 17 vs TV 8 = RV 9 → significant bonus
      Assert.IsTrue(result.FinalSV > result.SV);
    }

    [TestMethod]
    public void FullRangedAttackFlow_DifficultShot_ManyModifiers()
    {
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(4); // Good roll but may not overcome modifiers
      var resolver = new RangedAttackResolver(roller);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 6, // Long range = TV 10
        TargetMoving = true,    // +2
        Cover = CoverType.ThreeQuarters, // +2
        TargetSize = TargetSize.Small,   // +1
        AttackerMoving = true   // +2
        // Total TV = 10 + 2 + 2 + 1 + 2 = 17
      };

      var result = resolver.Resolve(request);

      Assert.AreEqual(10, result.BaseTV);
      Assert.AreEqual(7, result.TVModifiers);
      Assert.AreEqual(17, result.TV);
      // AV = 10 + 4 = 14 vs TV 17 = SV -3
      Assert.IsFalse(result.IsHit);
      Assert.AreEqual(-3, result.SV);
    }

    [TestMethod]
    public void FullRangedAttackFlow_WithAiming()
    {
      var aimState = new AimState();
      var roller = new DeterministicDiceRoller();
      roller.Queue4dFPlusResults(0); // Neutral roll
      roller.QueueDiceRolls(10); // Location
      var resolver = new RangedAttackResolver(roller);

      // Round 5: Aim at target
      aimState.StartAiming("enemy1", currentRound: 5);

      // Round 6: First action, attack same target
      bool hasAimBonus = aimState.CheckAimBonus("enemy1", currentRound: 6, actionsThisRound: 0);
      Assert.IsTrue(hasAimBonus);

      var request = new RangedAttackRequest
      {
        AttackerAS = 10,
        WeaponRanges = WeaponRanges.Shortbow,
        DistanceRangeValue = 3,
        HasAimBonus = hasAimBonus,
        ActionsThisRound = 0
      };

      var result = resolver.Resolve(request);

      // Effective AS = 10 + 2 (aim) = 12
      Assert.AreEqual(12, result.EffectiveAS);
      Assert.IsTrue(result.IsHit);

      // Consume the aim
      aimState.ConsumeAim();
      Assert.IsFalse(aimState.IsAiming);
    }

    #endregion
  }
}
