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
  [ExpectedException(typeof(InvalidOperationException))]
  public void CombatState_RecordParryDefense_NotInParryMode_Throws()
  {
    var state = new CombatState("char1");

    state.RecordParryDefense(); // Should throw
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
}
