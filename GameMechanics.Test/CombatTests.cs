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
}
