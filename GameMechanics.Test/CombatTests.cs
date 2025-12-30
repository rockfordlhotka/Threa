using System;
using GameMechanics;
using GameMechanics.Combat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class CombatTests
{
  #region HitLocation Tests

  [TestMethod]
  public void HitLocation_Roll1_ReturnsHead()
  {
    var location = HitLocationCalculator.MapRollToLocation(1);
    Assert.AreEqual(HitLocation.Head, location);
  }

  [TestMethod]
  [DataRow(2)]
  [DataRow(6)]
  [DataRow(12)]
  public void HitLocation_Roll2To12_ReturnsTorso(int roll)
  {
    var location = HitLocationCalculator.MapRollToLocation(roll);
    Assert.AreEqual(HitLocation.Torso, location);
  }

  [TestMethod]
  [DataRow(13)]
  [DataRow(14)]
  public void HitLocation_Roll13To14_ReturnsLeftArm(int roll)
  {
    var location = HitLocationCalculator.MapRollToLocation(roll);
    Assert.AreEqual(HitLocation.LeftArm, location);
  }

  [TestMethod]
  [DataRow(15)]
  [DataRow(16)]
  public void HitLocation_Roll15To16_ReturnsRightArm(int roll)
  {
    var location = HitLocationCalculator.MapRollToLocation(roll);
    Assert.AreEqual(HitLocation.RightArm, location);
  }

  [TestMethod]
  [DataRow(17)]
  [DataRow(20)]
  public void HitLocation_Roll17To20_ReturnsLeftLeg(int roll)
  {
    var location = HitLocationCalculator.MapRollToLocation(roll);
    Assert.AreEqual(HitLocation.LeftLeg, location);
  }

  [TestMethod]
  [DataRow(21)]
  [DataRow(24)]
  public void HitLocation_Roll21To24_ReturnsRightLeg(int roll)
  {
    var location = HitLocationCalculator.MapRollToLocation(roll);
    Assert.AreEqual(HitLocation.RightLeg, location);
  }

  [TestMethod]
  public void HitLocation_ProbabilitiesAddToOne()
  {
    double total = 0;
    foreach (HitLocation location in Enum.GetValues<HitLocation>())
    {
      total += HitLocationCalculator.GetLocationProbability(location);
    }
    Assert.AreEqual(1.0, total, 0.001);
  }

  [TestMethod]
  public void HitLocation_HeadProbability_IsCorrect()
  {
    double expected = 1.0 / 24.0; // 4.17%
    double actual = HitLocationCalculator.GetLocationProbability(HitLocation.Head);
    Assert.AreEqual(expected, actual, 0.001);
  }

  [TestMethod]
  public void HitLocation_TorsoProbability_IsCorrect()
  {
    double expected = 11.0 / 24.0; // 45.83%
    double actual = HitLocationCalculator.GetLocationProbability(HitLocation.Torso);
    Assert.AreEqual(expected, actual, 0.001);
  }

  #endregion

  #region Physicality Bonus Tests

  [TestMethod]
  [DataRow(-10, -3, 3)]
  [DataRow(-9, -3, 3)]
  public void PhysicalityBonus_VeryLowRV_GivesPenalty(int rv, int expectedPenalty, int expectedDuration)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(0, result.SVModifier);
    Assert.AreEqual(expectedPenalty, result.AttackerAVPenalty);
    Assert.AreEqual(expectedDuration, result.PenaltyDurationRounds);
  }

  [TestMethod]
  [DataRow(-2)]
  [DataRow(-1)]
  [DataRow(0)]
  [DataRow(1)]
  public void PhysicalityBonus_NeutralRV_NoEffect(int rv)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(0, result.SVModifier);
    Assert.AreEqual(0, result.AttackerAVPenalty);
  }

  [TestMethod]
  [DataRow(2, 1)]
  [DataRow(3, 1)]
  public void PhysicalityBonus_LowPositiveRV_GivesPlus1SV(int rv, int expectedBonus)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(expectedBonus, result.SVModifier);
    Assert.AreEqual(0, result.AttackerAVPenalty);
  }

  [TestMethod]
  [DataRow(4, 2)]
  [DataRow(7, 2)]
  public void PhysicalityBonus_MediumPositiveRV_GivesPlus2SV(int rv, int expectedBonus)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(expectedBonus, result.SVModifier);
  }

  [TestMethod]
  [DataRow(8, 3)]
  [DataRow(11, 3)]
  public void PhysicalityBonus_HighPositiveRV_GivesPlus3SV(int rv, int expectedBonus)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(expectedBonus, result.SVModifier);
  }

  [TestMethod]
  [DataRow(12, 4)]
  [DataRow(18, 4)]
  public void PhysicalityBonus_VeryHighPositiveRV_GivesPlus4SV(int rv, int expectedBonus)
  {
    var result = CombatResultTables.GetPhysicalityBonus(rv);
    Assert.AreEqual(expectedBonus, result.SVModifier);
  }

  #endregion

  #region Damage Table Tests

  [TestMethod]
  public void Damage_NegativeSV_NoDamage()
  {
    var result = CombatResultTables.GetDamage(-1);
    Assert.AreEqual(0, result.FatigueDamage);
    Assert.AreEqual(0, result.VitalityDamage);
    Assert.IsFalse(result.CausesWound);
  }

  [TestMethod]
  public void Damage_SV0_GlancingBlow()
  {
    var result = CombatResultTables.GetDamage(0);
    Assert.AreEqual(1, result.FatigueDamage);
    Assert.AreEqual(0, result.VitalityDamage);
    Assert.IsFalse(result.CausesWound);
  }

  [TestMethod]
  public void Damage_SV4_StartsVitalityDamage()
  {
    var result = CombatResultTables.GetDamage(4);
    Assert.AreEqual(5, result.FatigueDamage);
    Assert.AreEqual(1, result.VitalityDamage);
    Assert.IsFalse(result.CausesWound);
  }

  [TestMethod]
  public void Damage_SV6_CausesWound()
  {
    var result = CombatResultTables.GetDamage(6);
    Assert.IsTrue(result.CausesWound);
    Assert.AreEqual(7, result.FatigueDamage);
    Assert.AreEqual(3, result.VitalityDamage);
  }

  [TestMethod]
  public void Damage_HighSV_ScalesAppropriately()
  {
    var result = CombatResultTables.GetDamage(10);
    Assert.IsTrue(result.CausesWound);
    Assert.IsTrue(result.FatigueDamage > 8);
    Assert.IsTrue(result.VitalityDamage > 5);
  }

  #endregion

  #region AttackRequest Tests

  [TestMethod]
  public void AttackRequest_NoModifiers_ReturnsBaseAS()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12
    };

    Assert.AreEqual(12, request.GetEffectiveAS());
  }

  [TestMethod]
  public void AttackRequest_FirstAction_NoPenalty()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      ActionsThisRound = 0
    };

    Assert.AreEqual(0, request.CalculateTotalModifier());
  }

  [TestMethod]
  public void AttackRequest_SecondAction_Minus1Penalty()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      ActionsThisRound = 1
    };

    Assert.AreEqual(-1, request.CalculateTotalModifier());
    Assert.AreEqual(11, request.GetEffectiveAS());
  }

  [TestMethod]
  public void AttackRequest_ThirdAction_StillOnlyMinus1Penalty()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      ActionsThisRound = 2
    };

    // Multiple action penalty is NOT cumulative
    Assert.AreEqual(-1, request.CalculateTotalModifier());
  }

  [TestMethod]
  public void AttackRequest_WithAPBoost_IncreasesAS()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      APBoost = 2
    };

    Assert.AreEqual(14, request.GetEffectiveAS());
  }

  [TestMethod]
  public void AttackRequest_WithFATBoost_IncreasesAS()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      FATBoost = 3
    };

    Assert.AreEqual(15, request.GetEffectiveAS());
  }

  [TestMethod]
  public void AttackRequest_BoostOffsetsPenalty()
  {
    var request = new AttackRequest
    {
      AttackerAS = 12,
      ActionsThisRound = 1, // -1 penalty
      FATBoost = 1          // +1 boost
    };

    Assert.AreEqual(0, request.CalculateTotalModifier());
    Assert.AreEqual(12, request.GetEffectiveAS());
  }

  [TestMethod]
  public void AttackRequest_MixedBoosts_Stack()
  {
    var request = new AttackRequest
    {
      AttackerAS = 10,
      APBoost = 2,
      FATBoost = 1
    };

    Assert.AreEqual(13, request.GetEffectiveAS());
  }

  #endregion

  #region AttackResolver Tests

  [TestMethod]
  public void AttackResolver_HighAV_Hits()
  {
    // Attacker AS 15, roll +2 = AV 17
    // Defender Dodge AS 10, passive = TV 9
    // SV = 17 - 9 = 8 (hit)
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2, 0)  // Attack roll +2, Physicality roll 0
      .QueueDiceRolls(12);        // Hit location (torso)

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 15,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 10);

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.IsHit);
    Assert.AreEqual(17, result.AV);
    Assert.AreEqual(9, result.TV);
    Assert.AreEqual(8, result.SV);
    Assert.IsNotNull(result.HitLocation);
    Assert.IsNotNull(result.Damage);
  }

  [TestMethod]
  public void AttackResolver_LowAV_Misses()
  {
    // Attacker AS 8, roll -2 = AV 6
    // Defender Dodge AS 10, passive = TV 9
    // SV = 6 - 9 = -3 (miss)
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(-2);

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 8,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 10);

    var result = resolver.Resolve(request);

    Assert.IsFalse(result.IsHit);
    Assert.AreEqual(6, result.AV);
    Assert.AreEqual(9, result.TV);
    Assert.AreEqual(-3, result.SV);
    Assert.IsNull(result.HitLocation);
    Assert.IsNull(result.Damage);
  }

  [TestMethod]
  public void AttackResolver_ExactlyMatchTV_Hits()
  {
    // SV = 0 is a hit (barely)
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0)  // Attack roll 0, Physicality 0
      .QueueDiceRolls(5);         // Hit location

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 10,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 11); // TV = 10

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.IsHit);
    Assert.AreEqual(0, result.SV);
  }

  [TestMethod]
  public void AttackResolver_PhysicalityBonus_AppliedToFinalSV()
  {
    // Attack SV = 2
    // Physicality AS 12 + roll 0 = 12, RV = 12 - 8 = 4, gives +2 SV
    // Final SV = 2 + 2 = 4
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0)  // Attack roll 0, Physicality roll 0
      .QueueDiceRolls(5);         // Hit location

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 12,
      attackerPhysicalityAS: 12, // +4 RV = +2 SV bonus
      defenderDodgeAS: 11);      // TV = 10, SV = 2

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.IsHit);
    Assert.AreEqual(2, result.SV);
    Assert.AreEqual(4, result.PhysicalityRV);
    Assert.AreEqual(2, result.PhysicalityBonus!.SVModifier);
    Assert.AreEqual(4, result.FinalSV);
  }

  [TestMethod]
  public void AttackResolver_MultipleActionPenalty_Applied()
  {
    // Attacker AS 12, second action (-1), roll 0 = AV 11
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0)  // Attack roll 0, Physicality 0
      .QueueDiceRolls(5);         // Hit location

    var resolver = new AttackResolver(diceRoller);
    var request = new AttackRequest
    {
      AttackerAS = 12,
      AttackerPhysicalityAS = 10,
      DefenderDodgeAS = 10,
      ActionsThisRound = 1 // Second action
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(11, result.EffectiveAS);
    Assert.AreEqual(11, result.AV);
  }

  [TestMethod]
  public void AttackResolver_WithBoost_IncreasesAV()
  {
    // Attacker AS 10, FAT boost 2 = effective 12, roll 0 = AV 12
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0)  // Attack roll 0, Physicality 0
      .QueueDiceRolls(5);         // Hit location

    var resolver = new AttackResolver(diceRoller);
    var request = new AttackRequest
    {
      AttackerAS = 10,
      AttackerPhysicalityAS = 10,
      DefenderDodgeAS = 10,
      FATBoost = 2
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(12, result.EffectiveAS);
    Assert.AreEqual(12, result.AV);
  }

  [TestMethod]
  public void AttackResolver_ResolveWithTV_UsesProvidedTV()
  {
    // Test the overload that accepts an explicit TV
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);  // Attack roll

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 12,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 10); // Would give TV 9 passively

    // But we pass TV 15 explicitly (simulating active defense roll)
    var result = resolver.ResolveWithTV(request, tv: 15);

    Assert.IsFalse(result.IsHit);
    Assert.AreEqual(15, result.TV);
    Assert.AreEqual(-3, result.SV);
  }

  [TestMethod]
  public void AttackResolver_HitLocation_IsReturned()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2, 0)  // Attack roll (will hit), Physicality
      .QueueDiceRolls(1);         // Hit location = Head (roll 1)

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(15, 10, 10);

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.IsHit);
    Assert.AreEqual(HitLocation.Head, result.HitLocation);
  }

  [TestMethod]
  public void AttackResolver_Damage_IsCalculated()
  {
    // High SV should cause wound
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(4, 2)  // Attack roll +4, Physicality roll +2
      .QueueDiceRolls(5);         // Hit location

    var resolver = new AttackResolver(diceRoller);
    var request = AttackRequest.Create(
      attackerAS: 15,
      attackerPhysicalityAS: 12,
      defenderDodgeAS: 8); // TV = 7

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.IsHit);
    Assert.IsNotNull(result.Damage);
    Assert.IsTrue(result.FinalSV >= 6); // Should cause wound
    Assert.IsTrue(result.Damage.CausesWound);
  }

  #endregion

  #region DefenseResolver Tests

  [TestMethod]
  public void DefenseResolver_Passive_ReturnsDodgeASMinus1()
  {
    var diceRoller = new DeterministicDiceRoller();
    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.Passive(dodgeAS: 12);
    var result = resolver.Resolve(request);

    Assert.AreEqual(DefenseType.Passive, result.DefenseType);
    Assert.AreEqual(11, result.TV);
    Assert.AreEqual(12, result.AS);
    Assert.IsNull(result.DefenseRoll);
    Assert.IsFalse(result.CostsAction);
    Assert.IsTrue(result.IsValid);
  }

  [TestMethod]
  public void DefenseResolver_ActiveDodge_AddsDiceRoll()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(3);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.Dodge(dodgeAS: 10);
    var result = resolver.Resolve(request);

    Assert.AreEqual(DefenseType.Dodge, result.DefenseType);
    Assert.AreEqual(13, result.TV); // 10 + 3
    Assert.AreEqual(10, result.AS);
    Assert.AreEqual(3, result.DefenseRoll);
    Assert.IsTrue(result.CostsAction);
    Assert.IsTrue(result.IsValid);
  }

  [TestMethod]
  public void DefenseResolver_Parry_NotInParryMode_CostsAction()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.Parry(parryAS: 14, isInParryMode: false);
    var result = resolver.Resolve(request);

    Assert.AreEqual(DefenseType.Parry, result.DefenseType);
    Assert.AreEqual(16, result.TV); // 14 + 2
    Assert.IsTrue(result.CostsAction);
  }

  [TestMethod]
  public void DefenseResolver_Parry_InParryMode_IsFree()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.Parry(parryAS: 14, isInParryMode: true);
    var result = resolver.Resolve(request);

    Assert.AreEqual(DefenseType.Parry, result.DefenseType);
    Assert.AreEqual(16, result.TV);
    Assert.IsFalse(result.CostsAction); // Free when in parry mode
  }

  [TestMethod]
  public void DefenseResolver_Parry_AgainstRanged_IsInvalid()
  {
    var diceRoller = new DeterministicDiceRoller();
    var resolver = new DefenseResolver(diceRoller);

    var request = new DefenseRequest
    {
      DefenseType = DefenseType.Parry,
      ParryAS = 14,
      IsRangedAttack = true
    };
    var result = resolver.Resolve(request);

    Assert.IsFalse(result.IsValid);
    Assert.IsNotNull(result.ErrorMessage);
    Assert.IsTrue(result.ErrorMessage.Contains("ranged"));
  }

  [TestMethod]
  public void DefenseResolver_ShieldBlock_SuccessfulRoll()
  {
    // Shield AS 12 + roll 0 = 12 >= TV 8, success with RV 4
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.ShieldBlock(shieldAS: 12);
    var result = resolver.ResolveShieldBlock(request, baseTV: 10);

    Assert.AreEqual(DefenseType.ShieldBlock, result.DefenseType);
    Assert.AreEqual(10, result.TV); // Base TV unchanged
    Assert.IsTrue(result.ShieldBlockSuccess);
    Assert.AreEqual(4, result.ShieldBlockRV); // 12 - 8 = 4
    Assert.IsFalse(result.CostsAction); // Shield block is free
  }

  [TestMethod]
  public void DefenseResolver_ShieldBlock_FailedRoll()
  {
    // Shield AS 6 + roll -2 = 4 < TV 8, failure
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(-2);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.ShieldBlock(shieldAS: 6);
    var result = resolver.ResolveShieldBlock(request, baseTV: 10);

    Assert.IsFalse(result.ShieldBlockSuccess);
    Assert.IsNull(result.ShieldBlockRV); // No RV on failure
    Assert.AreEqual(10, result.TV); // Base TV unchanged
  }

  [TestMethod]
  public void DefenseResolver_ResolveWithShield_BothResultsReturned()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2, 1); // Dodge roll, then shield roll

    var resolver = new DefenseResolver(diceRoller);

    var primaryRequest = DefenseRequest.Dodge(dodgeAS: 10);
    var (primary, shield) = resolver.ResolveWithShield(primaryRequest, hasShield: true, shieldAS: 8);

    Assert.IsNotNull(primary);
    Assert.AreEqual(DefenseType.Dodge, primary.DefenseType);
    Assert.AreEqual(12, primary.TV); // 10 + 2

    Assert.IsNotNull(shield);
    Assert.AreEqual(DefenseType.ShieldBlock, shield.DefenseType);
    Assert.AreEqual(12, shield.TV); // Uses primary's TV
    Assert.IsTrue(shield.ShieldBlockSuccess); // 8 + 1 = 9 >= 8
  }

  [TestMethod]
  public void DefenseResolver_ResolveWithShield_NoShield_ReturnsNullShield()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2);

    var resolver = new DefenseResolver(diceRoller);

    var primaryRequest = DefenseRequest.Dodge(dodgeAS: 10);
    var (primary, shield) = resolver.ResolveWithShield(primaryRequest, hasShield: false);

    Assert.IsNotNull(primary);
    Assert.IsNull(shield);
  }

  [TestMethod]
  public void DefenseResolver_NegativeDodgeRoll_StillCalculates()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(-3);

    var resolver = new DefenseResolver(diceRoller);

    var request = DefenseRequest.Dodge(dodgeAS: 10);
    var result = resolver.Resolve(request);

    Assert.AreEqual(7, result.TV); // 10 + (-3)
    Assert.AreEqual(-3, result.DefenseRoll);
  }

  #endregion

  #region CombatState Tests

  [TestMethod]
  public void CombatState_NewState_HasNoActions()
  {
    var state = new CombatState("char1");

    Assert.AreEqual(0, state.ActionsThisRound);
    Assert.IsFalse(state.HasActed);
    Assert.IsFalse(state.IsInParryMode);
    Assert.IsFalse(state.WillHaveMultipleActionPenalty);
  }

  [TestMethod]
  public void CombatState_RecordAction_IncrementsCount()
  {
    var state = new CombatState("char1");

    state.RecordAction();

    Assert.AreEqual(1, state.ActionsThisRound);
    Assert.IsTrue(state.HasActed);
    Assert.IsTrue(state.WillHaveMultipleActionPenalty);
  }

  [TestMethod]
  public void CombatState_MultipleActions_TrackedCorrectly()
  {
    var state = new CombatState("char1");

    state.RecordAction();
    state.RecordAction();
    state.RecordAction();

    Assert.AreEqual(3, state.ActionsThisRound);
  }

  [TestMethod]
  public void CombatState_EnterParryMode_SetsState()
  {
    var state = new CombatState("char1");

    state.EnterParryMode("sword-skill");

    Assert.IsTrue(state.IsInParryMode);
    Assert.AreEqual("sword-skill", state.ParrySkillId);
    Assert.AreEqual(1, state.ActionsThisRound); // Entering parry mode is an action
  }

  [TestMethod]
  public void CombatState_NonParryAction_ExitsParryMode()
  {
    var state = new CombatState("char1");
    state.EnterParryMode("sword-skill");

    // Take a non-parry action (like attacking)
    state.RecordAction(isParryDefense: false);

    Assert.IsFalse(state.IsInParryMode);
    Assert.IsNull(state.ParrySkillId);
  }

  [TestMethod]
  public void CombatState_ParryDefense_StaysInParryMode()
  {
    var state = new CombatState("char1");
    state.EnterParryMode("sword-skill");

    // Record parry defenses
    state.RecordAction(isParryDefense: true);
    state.RecordAction(isParryDefense: true);

    Assert.IsTrue(state.IsInParryMode);
    Assert.AreEqual("sword-skill", state.ParrySkillId);
  }

  [TestMethod]
  public void CombatState_RecordParryDefense_WhileInParryMode_IsFree()
  {
    var state = new CombatState("char1");
    state.EnterParryMode("sword-skill"); // Actions = 1

    state.RecordParryDefense();
    state.RecordParryDefense();

    // Parry defenses while in parry mode don't increment action count
    Assert.AreEqual(1, state.ActionsThisRound);
    Assert.IsTrue(state.IsInParryMode);
  }

  [TestMethod]
  public void CombatState_RecordParryDefense_NotInParryMode_Throws()
  {
    var state = new CombatState("char1");

    Assert.ThrowsExactly<InvalidOperationException>(() => state.RecordParryDefense());
  }

  [TestMethod]
  public void CombatState_StartNewRound_ResetsActions_KeepsParryMode()
  {
    var state = new CombatState("char1");
    state.EnterParryMode("sword-skill");
    state.RecordAction(isParryDefense: true);

    state.StartNewRound();

    Assert.AreEqual(0, state.ActionsThisRound);
    Assert.IsFalse(state.HasActed);
    Assert.IsTrue(state.IsInParryMode); // Parry mode persists across rounds
    Assert.AreEqual("sword-skill", state.ParrySkillId);
  }

  [TestMethod]
  public void CombatState_Reset_ClearsEverything()
  {
    var state = new CombatState("char1");
    state.EnterParryMode("sword-skill");
    state.RecordAction();

    state.Reset();

    Assert.AreEqual(0, state.ActionsThisRound);
    Assert.IsFalse(state.IsInParryMode);
    Assert.IsNull(state.ParrySkillId);
  }

  #endregion

  #region CombatStateManager Tests

  [TestMethod]
  public void CombatStateManager_GetState_CreatesIfNotExists()
  {
    var manager = new CombatStateManager();

    var state = manager.GetState("char1");

    Assert.IsNotNull(state);
    Assert.AreEqual("char1", state.CombatantId);
  }

  [TestMethod]
  public void CombatStateManager_GetState_ReturnsSameInstance()
  {
    var manager = new CombatStateManager();

    var state1 = manager.GetState("char1");
    state1.RecordAction();

    var state2 = manager.GetState("char1");

    Assert.AreSame(state1, state2);
    Assert.AreEqual(1, state2.ActionsThisRound);
  }

  [TestMethod]
  public void CombatStateManager_StartNewRound_ResetsAllCombatants()
  {
    var manager = new CombatStateManager();

    var state1 = manager.GetState("char1");
    var state2 = manager.GetState("char2");
    state1.RecordAction();
    state2.RecordAction();
    state2.RecordAction();

    manager.StartNewRound();

    Assert.AreEqual(0, state1.ActionsThisRound);
    Assert.AreEqual(0, state2.ActionsThisRound);
  }

  [TestMethod]
  public void CombatStateManager_RemoveCombatant_RemovesFromTracking()
  {
    var manager = new CombatStateManager();

    var state1 = manager.GetState("char1");
    state1.RecordAction();

    manager.RemoveCombatant("char1");

    var state2 = manager.GetState("char1");

    // Should be a new state, not the old one
    Assert.AreEqual(0, state2.ActionsThisRound);
  }

  [TestMethod]
  public void CombatStateManager_EndCombat_ClearsAllState()
  {
    var manager = new CombatStateManager();
    manager.GetState("char1");
    manager.GetState("char2");

    manager.EndCombat();

    // Getting combatant IDs should return empty after clearing
    var ids = manager.GetCombatantIds();
    Assert.AreEqual(0, System.Linq.Enumerable.Count(ids));
  }

  #endregion

  #region Integration Tests - Defense + Attack Resolution

  [TestMethod]
  public void Integration_PassiveDefense_AttackResolvesWithTV()
  {
    // Defender uses passive defense
    var defenseRoller = new DeterministicDiceRoller();
    var defenseResolver = new DefenseResolver(defenseRoller);

    var defenseRequest = DefenseRequest.Passive(dodgeAS: 12);
    var defenseResult = defenseResolver.Resolve(defenseRequest);

    Assert.AreEqual(11, defenseResult.TV);

    // Attacker resolves attack with that TV
    var attackRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0); // Attack roll

    var attackResolver = new AttackResolver(attackRoller);
    var attackRequest = AttackRequest.Create(
      attackerAS: 14,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 12);

    var attackResult = attackResolver.ResolveWithTV(attackRequest, defenseResult.TV);

    Assert.IsTrue(attackResult.IsHit);
    Assert.AreEqual(11, attackResult.TV);
    Assert.AreEqual(3, attackResult.SV); // 14 - 11 = 3
  }

  [TestMethod]
  public void Integration_ActiveDodge_IncreasesTV()
  {
    // Defender uses active dodge
    var defenseRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(4); // Good dodge roll

    var defenseResolver = new DefenseResolver(defenseRoller);
    var defenseResult = defenseResolver.Resolve(DefenseRequest.Dodge(dodgeAS: 12));

    Assert.AreEqual(16, defenseResult.TV); // 12 + 4

    // Same attack now misses
    var attackRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var attackResolver = new AttackResolver(attackRoller);
    var attackRequest = AttackRequest.Create(14, 10, 12);

    var attackResult = attackResolver.ResolveWithTV(attackRequest, defenseResult.TV);

    Assert.IsFalse(attackResult.IsHit);
    Assert.AreEqual(-2, attackResult.SV); // 14 - 16 = -2
  }

  [TestMethod]
  public void Integration_ParryMode_MultipleDefensesFree()
  {
    // Character enters parry mode
    var state = new CombatState("defender");
    state.EnterParryMode("sword-skill");

    Assert.AreEqual(1, state.ActionsThisRound); // Cost to enter

    // Multiple parry defenses are free
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2, 3, 1); // Three parry rolls

    var defenseResolver = new DefenseResolver(diceRoller);

    // First parry
    var result1 = defenseResolver.Resolve(DefenseRequest.Parry(parryAS: 14, isInParryMode: true));
    state.RecordParryDefense();

    // Second parry
    var result2 = defenseResolver.Resolve(DefenseRequest.Parry(parryAS: 14, isInParryMode: true));
    state.RecordParryDefense();

    // Third parry
    var result3 = defenseResolver.Resolve(DefenseRequest.Parry(parryAS: 14, isInParryMode: true));
    state.RecordParryDefense();

    Assert.AreEqual(16, result1.TV);
    Assert.AreEqual(17, result2.TV);
    Assert.AreEqual(15, result3.TV);

    Assert.IsFalse(result1.CostsAction);
    Assert.IsFalse(result2.CostsAction);
    Assert.IsFalse(result3.CostsAction);

    // Actions still at 1 (just the parry mode entry)
    Assert.AreEqual(1, state.ActionsThisRound);
    Assert.IsTrue(state.IsInParryMode);
  }

  [TestMethod]
  public void Integration_AttackBreaksParryMode()
  {
    var state = new CombatState("fighter");
    state.EnterParryMode("sword-skill");

    Assert.IsTrue(state.IsInParryMode);

    // Take an attack action
    state.RecordAction(isParryDefense: false);

    Assert.IsFalse(state.IsInParryMode);
    Assert.AreEqual(2, state.ActionsThisRound);
  }

  #endregion

  #region ArmorInfo Tests

  [TestMethod]
  public void ArmorInfo_CoversLocation_ReturnsTrueForCoveredLocation()
  {
    var armor = new ArmorInfo
    {
      Name = "Chainmail",
      CoveredLocations = [HitLocation.Torso, HitLocation.LeftArm, HitLocation.RightArm]
    };

    Assert.IsTrue(armor.CoversLocation(HitLocation.Torso));
    Assert.IsTrue(armor.CoversLocation(HitLocation.LeftArm));
    Assert.IsFalse(armor.CoversLocation(HitLocation.Head));
    Assert.IsFalse(armor.CoversLocation(HitLocation.LeftLeg));
  }

  [TestMethod]
  public void ArmorInfo_GetAbsorption_ReturnsCorrectValue()
  {
    var armor = new ArmorInfo
    {
      Absorption = new()
      {
        { DamageType.Cutting, 4 },
        { DamageType.Piercing, 2 },
        { DamageType.Bashing, 5 }
      }
    };

    Assert.AreEqual(4, armor.GetAbsorption(DamageType.Cutting));
    Assert.AreEqual(2, armor.GetAbsorption(DamageType.Piercing));
    Assert.AreEqual(0, armor.GetAbsorption(DamageType.Energy)); // Not defined
  }

  [TestMethod]
  public void ArmorInfo_ReduceDurability_DecreasesCorrectly()
  {
    var armor = new ArmorInfo
    {
      CurrentDurability = 50,
      MaxDurability = 100
    };

    int reduced = armor.ReduceDurability(10);

    Assert.AreEqual(10, reduced);
    Assert.AreEqual(40, armor.CurrentDurability);
    Assert.IsTrue(armor.IsIntact);
  }

  [TestMethod]
  public void ArmorInfo_ReduceDurability_CapsAtZero()
  {
    var armor = new ArmorInfo
    {
      CurrentDurability = 5,
      MaxDurability = 100
    };

    int reduced = armor.ReduceDurability(10);

    Assert.AreEqual(5, reduced); // Only 5 was available
    Assert.AreEqual(0, armor.CurrentDurability);
    Assert.IsFalse(armor.IsIntact);
  }

  [TestMethod]
  public void ArmorInfo_IsIntact_FalseWhenDurabilityZero()
  {
    var armor = new ArmorInfo
    {
      CurrentDurability = 0,
      MaxDurability = 100
    };

    Assert.IsFalse(armor.IsIntact);
  }

  #endregion

  #region ShieldInfo Tests

  [TestMethod]
  public void ShieldInfo_GetAbsorption_ReturnsCorrectValue()
  {
    var shield = new ShieldInfo
    {
      Absorption = new()
      {
        { DamageType.Bashing, 5 },
        { DamageType.Cutting, 4 },
        { DamageType.Projectile, 6 }
      }
    };

    Assert.AreEqual(5, shield.GetAbsorption(DamageType.Bashing));
    Assert.AreEqual(6, shield.GetAbsorption(DamageType.Projectile));
    Assert.AreEqual(0, shield.GetAbsorption(DamageType.Energy));
  }

  [TestMethod]
  public void ShieldInfo_ReduceDurability_WorksCorrectly()
  {
    var shield = new ShieldInfo
    {
      CurrentDurability = 30,
      MaxDurability = 50
    };

    shield.ReduceDurability(15);

    Assert.AreEqual(15, shield.CurrentDurability);
    Assert.IsTrue(shield.IsIntact);
  }

  #endregion

  #region DamageResolver Tests - Basic

  [TestMethod]
  public void DamageResolver_NoDefense_FullDamage()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0); // Armor skill check

    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 6,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 10,
      ArmorPieces = new()
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(6, result.IncomingSV);
    Assert.AreEqual(6, result.PenetratingSV);
    Assert.AreEqual(0, result.TotalAbsorbed);
    Assert.IsTrue(result.CausedWound);
  }

  [TestMethod]
  public void DamageResolver_ArmorAbsorbs_ReducesSV()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0); // Armor skill check

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "chainmail-1",
      Name = "Chainmail",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 4 } },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8, // AS 8 + roll 0 = 8, RV = 0, no bonus
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(8, result.IncomingSV);
    Assert.AreEqual(4, result.TotalAbsorbed);
    Assert.AreEqual(4, result.PenetratingSV);
    Assert.AreEqual(96, armor.CurrentDurability); // 4 durability lost
  }

  [TestMethod]
  public void DamageResolver_ArmorSkillBonus_IncreasesAbsorption()
  {
    // Armor AS 12 + roll 2 = 14, RV = 6, gives +2 absorption
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "plate-1",
      Name = "Plate",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 12,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(2, result.ArmorSkillBonus);
    Assert.AreEqual(7, result.TotalAbsorbed); // 5 base + 2 skill
    Assert.AreEqual(3, result.PenetratingSV);
  }

  [TestMethod]
  public void DamageResolver_ArmorSkillPenalty_DecreasesAbsorption()
  {
    // Armor AS 5 + roll -3 = 2, RV = -6, gives -2 absorption
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(-3);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "leather-1",
      Name = "Leather",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 3 } },
      CurrentDurability = 50,
      MaxDurability = 50,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 5,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 5,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(-2, result.ArmorSkillBonus);
    Assert.AreEqual(1, result.TotalAbsorbed); // 3 - 2 = 1
    Assert.AreEqual(4, result.PenetratingSV);
  }

  [TestMethod]
  public void DamageResolver_FullAbsorption_NoDamage()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "plate-1",
      Name = "Full Plate",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 10 } },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 5,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(5, result.TotalAbsorbed);
    Assert.IsTrue(result.FullyAbsorbed);
    Assert.AreEqual(0, result.FinalDamage.FatigueDamage);
    Assert.AreEqual(0, result.FinalDamage.VitalityDamage);
  }

  #endregion

  #region DamageResolver Tests - Shield

  [TestMethod]
  public void DamageResolver_ShieldBlock_AbsorbsFirst()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0); // Armor skill check

    var resolver = new DamageResolver(diceRoller);

    var shield = new ShieldInfo
    {
      ItemId = "shield-1",
      Name = "Round Shield",
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 50,
      MaxDurability = 50
    };

    var armor = new ArmorInfo
    {
      ItemId = "chainmail-1",
      Name = "Chainmail",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 4 } },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 12,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ShieldBlockSucceeded = true,
      ShieldBlockRV = 4, // Gives +2 bonus
      Shield = shield,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(2, result.AbsorptionSteps.Count);
    Assert.IsTrue(result.AbsorptionSteps[0].IsShield);
    Assert.IsFalse(result.AbsorptionSteps[1].IsShield);

    // Shield: base 5 + bonus 2 = 7
    Assert.AreEqual(7, result.AbsorptionSteps[0].TotalAbsorbed);
    Assert.AreEqual(43, shield.CurrentDurability);

    // Armor: 12 - 7 = 5 remaining, absorbs 4
    Assert.AreEqual(4, result.AbsorptionSteps[1].TotalAbsorbed);
    Assert.AreEqual(96, armor.CurrentDurability);

    // Final: 12 - 7 - 4 = 1
    Assert.AreEqual(1, result.PenetratingSV);
  }

  [TestMethod]
  public void DamageResolver_ShieldBlockFailed_NoShieldAbsorption()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var shield = new ShieldInfo
    {
      ItemId = "shield-1",
      Name = "Shield",
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 50,
      MaxDurability = 50
    };

    var request = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ShieldBlockSucceeded = false, // Failed shield block
      Shield = shield,
      ArmorPieces = new()
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.AbsorptionSteps.Count);
    Assert.AreEqual(50, shield.CurrentDurability); // Shield not damaged
    Assert.AreEqual(8, result.PenetratingSV);
  }

  #endregion

  #region DamageResolver Tests - Layered Armor

  [TestMethod]
  public void DamageResolver_MultipleArmorLayers_AbsorbOuterFirst()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var gambeson = new ArmorInfo
    {
      ItemId = "gambeson-1",
      Name = "Gambeson",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 2 } },
      CurrentDurability = 30,
      MaxDurability = 30,
      LayerOrder = 1 // Inner
    };

    var chainmail = new ArmorInfo
    {
      ItemId = "chainmail-1",
      Name = "Chainmail",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 4 } },
      CurrentDurability = 80,
      MaxDurability = 100,
      LayerOrder = 0 // Outer
    };

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { gambeson, chainmail } // Wrong order in list
    };

    var result = resolver.Resolve(request);

    // Should process in layer order (chainmail first, then gambeson)
    Assert.AreEqual(2, result.AbsorptionSteps.Count);
    Assert.AreEqual("Chainmail", result.AbsorptionSteps[0].ItemName);
    Assert.AreEqual("Gambeson", result.AbsorptionSteps[1].ItemName);

    Assert.AreEqual(4, result.AbsorptionSteps[0].TotalAbsorbed);
    Assert.AreEqual(2, result.AbsorptionSteps[1].TotalAbsorbed);
    Assert.AreEqual(4, result.PenetratingSV);
  }

  [TestMethod]
  public void DamageResolver_ArmorSkillBonus_OnlyFirstLayer()
  {
    // Armor AS 12 + roll 2 = 14, RV = 6, gives +2 absorption (first layer only)
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2);

    var resolver = new DamageResolver(diceRoller);

    var outer = new ArmorInfo
    {
      ItemId = "outer-1",
      Name = "Outer Layer",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 3 } },
      CurrentDurability = 50,
      MaxDurability = 50,
      LayerOrder = 0
    };

    var inner = new ArmorInfo
    {
      ItemId = "inner-1",
      Name = "Inner Layer",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 2 } },
      CurrentDurability = 30,
      MaxDurability = 30,
      LayerOrder = 1
    };

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 12,
      ArmorPieces = new() { outer, inner }
    };

    var result = resolver.Resolve(request);

    // Outer: 3 base + 2 skill = 5
    Assert.AreEqual(5, result.AbsorptionSteps[0].TotalAbsorbed);
    Assert.AreEqual(2, result.AbsorptionSteps[0].SkillBonus);

    // Inner: 2 base + 0 skill = 2
    Assert.AreEqual(2, result.AbsorptionSteps[1].TotalAbsorbed);
    Assert.AreEqual(0, result.AbsorptionSteps[1].SkillBonus);

    Assert.AreEqual(3, result.PenetratingSV);
  }

  [TestMethod]
  public void DamageResolver_ArmorNotCoveringLocation_NotUsed()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "greaves-1",
      Name = "Leg Greaves",
      CoveredLocations = [HitLocation.LeftLeg, HitLocation.RightLeg],
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 50,
      MaxDurability = 50,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 6,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso, // Armor doesn't cover torso
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.AbsorptionSteps.Count);
    Assert.AreEqual(6, result.PenetratingSV);
    Assert.AreEqual(50, armor.CurrentDurability); // Not damaged
  }

  #endregion

  #region DamageResolver Tests - Equipment Destruction

  [TestMethod]
  public void DamageResolver_ArmorDestroyed_MarkedInRecord()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "worn-leather-1",
      Name = "Worn Leather",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 10 } },
      CurrentDurability = 3, // Low durability
      MaxDurability = 50,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(1, result.AbsorptionSteps.Count);
    Assert.IsTrue(result.AbsorptionSteps[0].ItemDestroyed);
    Assert.AreEqual(3, result.AbsorptionSteps[0].DurabilityLost);
    Assert.AreEqual(3, result.AbsorptionSteps[0].TotalAbsorbed); // Limited by durability
    Assert.AreEqual(0, armor.CurrentDurability);
    Assert.IsFalse(armor.IsIntact);
  }

  [TestMethod]
  public void DamageResolver_DestroyedArmorNotUsed_InSubsequentCalls()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0); // Two calls

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "fragile-1",
      Name = "Fragile Armor",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 2,
      MaxDurability = 50,
      LayerOrder = 0
    };

    // First hit destroys armor
    var request1 = new DamageRequest
    {
      IncomingSV = 6,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result1 = resolver.Resolve(request1);
    Assert.IsFalse(armor.IsIntact);

    // Second hit - armor no longer provides protection
    var request2 = new DamageRequest
    {
      IncomingSV = 4,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result2 = resolver.Resolve(request2);

    Assert.AreEqual(0, result2.AbsorptionSteps.Count);
    Assert.AreEqual(4, result2.PenetratingSV);
  }

  #endregion

  #region DamageResolver Tests - Damage Type Matching

  [TestMethod]
  public void DamageResolver_DifferentDamageTypes_UseCorrectAbsorption()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0, 0);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "mixed-1",
      Name = "Mixed Armor",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new()
      {
        { DamageType.Cutting, 5 },
        { DamageType.Piercing, 2 },
        { DamageType.Bashing, 6 }
      },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    // Test cutting
    var cutRequest = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var cutResult = resolver.Resolve(cutRequest);
    Assert.AreEqual(5, cutResult.TotalAbsorbed);

    // Reset durability for next test
    armor.CurrentDurability = 100;

    // Test piercing
    var pierceRequest = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var pierceResult = resolver.Resolve(pierceRequest);
    Assert.AreEqual(2, pierceResult.TotalAbsorbed);
  }

  [TestMethod]
  public void DamageResolver_UnknownDamageType_ZeroAbsorption()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "metal-1",
      Name = "Metal Armor",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new()
      {
        { DamageType.Cutting, 5 },
        { DamageType.Piercing, 3 }
        // No Energy absorption defined
      },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 6,
      DamageType = DamageType.Energy, // Not defined for this armor
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.TotalAbsorbed);
    Assert.AreEqual(6, result.PenetratingSV);
  }

  #endregion

  #region DamageResolver Tests - Damage Class

  [TestMethod]
  public void DamageResolver_HigherClassArmor_AbsorbsMoreLowerClassDamage()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    // Class 2 armor vs Class 1 attack
    var armor = new ArmorInfo
    {
      ItemId = "heavy-1",
      Name = "Heavy Plate",
      CoveredLocations = [HitLocation.Torso],
      DamageClass = 2,
      Absorption = new() { { DamageType.Cutting, 5 } },
      CurrentDurability = 200,
      MaxDurability = 200,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 1, // Lower class attack
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    // Class 2 armor absorbs class 1 damage at 10x effectiveness
    Assert.IsTrue(result.FullyAbsorbed);
  }

  [TestMethod]
  public void DamageResolver_LowerClassArmor_AbsorbsLessHigherClassDamage()
  {
    var diceRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0);

    var resolver = new DamageResolver(diceRoller);

    // Class 1 armor vs Class 2 attack
    var armor = new ArmorInfo
    {
      ItemId = "light-1",
      Name = "Light Armor",
      CoveredLocations = [HitLocation.Torso],
      DamageClass = 1,
      Absorption = new() { { DamageType.Cutting, 30 } }, // High absorption
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var request = new DamageRequest
    {
      IncomingSV = 8,
      DamageType = DamageType.Cutting,
      DamageClass = 2, // Higher class attack
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new() { armor }
    };

    var result = resolver.Resolve(request);

    // Class 1 armor only absorbs 3 from class 2 (30 / 10 = 3)
    Assert.AreEqual(3, result.TotalAbsorbed);
    Assert.AreEqual(5, result.PenetratingSV);
  }

  #endregion

  #region Integration Tests - Full Combat Flow

  [TestMethod]
  public void Integration_FullCombatFlow_AttackDefenseDamage()
  {
    // Attacker hits defender, damage goes through shield and armor
    var attackRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(2, 0)  // Attack roll, Physicality
      .QueueDiceRolls(5);         // Hit location (torso)

    var attackResolver = new AttackResolver(attackRoller);
    var attackResult = attackResolver.Resolve(AttackRequest.Create(
      attackerAS: 14,
      attackerPhysicalityAS: 10,
      defenderDodgeAS: 8)); // Passive TV = 7

    Assert.IsTrue(attackResult.IsHit);
    Assert.AreEqual(HitLocation.Torso, attackResult.HitLocation);

    // Defender resolves shield block (succeeded in Phase 2)
    var shield = new ShieldInfo
    {
      ItemId = "shield-1",
      Name = "Round Shield",
      Absorption = new() { { DamageType.Cutting, 4 } },
      CurrentDurability = 50,
      MaxDurability = 50
    };

    var armor = new ArmorInfo
    {
      ItemId = "chainmail-1",
      Name = "Chainmail",
      CoveredLocations = [HitLocation.Torso],
      Absorption = new() { { DamageType.Cutting, 3 } },
      CurrentDurability = 80,
      MaxDurability = 100,
      LayerOrder = 0
    };

    var damageRoller = new DeterministicDiceRoller()
      .Queue4dFPlusResults(0); // Armor skill

    var damageResolver = new DamageResolver(damageRoller);
    var damageRequest = DamageRequest.FromAttack(
      attackResult,
      DamageType.Cutting,
      damageClass: 1,
      defenderArmorAS: 10,
      armorPieces: new() { armor },
      shield: shield,
      shieldBlockSucceeded: true,
      shieldBlockRV: 2); // +1 bonus

    var damageResult = damageResolver.Resolve(damageRequest);

    // Verify the full chain
    Assert.AreEqual(attackResult.FinalSV, damageResult.IncomingSV);
    Assert.AreEqual(2, damageResult.AbsorptionSteps.Count);
    Assert.IsTrue(damageResult.AbsorptionSteps[0].IsShield);
    Assert.IsFalse(damageResult.AbsorptionSteps[1].IsShield);

    // Shield absorbed some, armor absorbed some, rest penetrates
    Assert.IsTrue(damageResult.PenetratingSV < damageResult.IncomingSV);
  }

  #endregion
}
