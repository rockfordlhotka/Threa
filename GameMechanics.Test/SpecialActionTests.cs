using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMechanics.Combat;
using GameMechanics.Combat.SpecialActions;

namespace GameMechanics.Test;

[TestClass]
public class SpecialActionTests
{
  private static DeterministicDiceRoller CreateDice(params int[] results)
  {
    return new DeterministicDiceRoller().Queue4dFPlusResults(results);
  }

  #region Knockback Tests

  [TestMethod]
  public void Knockback_NoASPenalty()
  {
    // Verify Knockback has 0 AS penalty
    var penalty = SpecialActionModifiers.GetPenalty(SpecialActionType.Knockback);
    Assert.AreEqual(0, penalty);
  }

  [TestMethod]
  public void Knockback_Success_DurationFromRVETable()
  {
    var dice = CreateDice(2); // Roll of +2
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateKnockback(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6 // TV = 5
    );

    var result = resolver.Resolve(request);

    // AS 10, Roll +2 = AV 12, TV 5 → SV 7
    Assert.IsTrue(result.Success);
    Assert.AreEqual(7, result.SV);
    Assert.AreEqual(5.0, result.KnockbackDurationSeconds);
    Assert.IsFalse(result.KnockedProne);
  }

  [TestMethod]
  public void Knockback_Miss_NoDuration()
  {
    var dice = CreateDice(-4); // Terrible roll
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateKnockback(
      attackerAS: 5,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 10 // TV = 9
    );

    var result = resolver.Resolve(request);

    // AS 5, Roll -4 = AV 1, TV 9 → SV -8
    Assert.IsFalse(result.Success);
    Assert.IsNull(result.KnockbackDurationSeconds);
  }

  [TestMethod]
  public void Knockback_Critical_KnocksProne()
  {
    var dice = CreateDice(5); // Excellent roll
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateKnockback(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 4 // TV = 3
    );

    var result = resolver.Resolve(request);

    // AS 10, Roll +5 = AV 15, TV 3 → SV 12
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.SV >= KnockbackTable.CriticalThreshold);
    Assert.IsTrue(result.KnockedProne);
    Assert.IsTrue(result.IsCritical);
  }

  [TestMethod]
  public void Knockback_WithoutCapability_Throws()
  {
    var dice = CreateDice(2);
    var resolver = new SpecialActionResolver(dice);

    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.Knockback,
      AttackerAS = 10,
      DefenderDodgeAS = 6,
      WeaponHasKnockback = false // No knockback capability
    };

    Assert.ThrowsExactly<InvalidOperationException>(() => resolver.Resolve(request));
  }

  [TestMethod]
  public void Knockback_NoDamageDealt()
  {
    var dice = CreateDice(3);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateKnockback(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6
    );

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.Success);
    Assert.IsNull(result.Damage); // No damage for knockback
  }

  [TestMethod]
  [DataRow(0, 0.5)]
  [DataRow(1, 1.0)]
  [DataRow(2, 1.5)]
  [DataRow(3, 2.0)]
  [DataRow(5, 3.0)]
  [DataRow(7, 5.0)]
  [DataRow(10, 8.0)]
  public void KnockbackTable_DurationMapping(int sv, double expectedSeconds)
  {
    Assert.AreEqual(expectedSeconds, KnockbackTable.GetDurationSeconds(sv));
  }

  [TestMethod]
  public void KnockbackTable_NegativeSV_ZeroDuration()
  {
    Assert.AreEqual(0, KnockbackTable.GetDurationSeconds(-1));
    Assert.AreEqual(0, KnockbackTable.GetDurationSeconds(-5));
  }

  [TestMethod]
  [DataRow(0, 1)]
  [DataRow(3, 1)]
  [DataRow(5, 1)]
  [DataRow(6, 2)]
  [DataRow(8, 2)]
  [DataRow(10, 3)]
  public void KnockbackTable_DurationRounds(int sv, int expectedRounds)
  {
    Assert.AreEqual(expectedRounds, KnockbackTable.GetDurationRounds(sv));
  }

  #endregion

  #region Disarm Tests

  [TestMethod]
  public void Disarm_HasMinusTwoASPenalty()
  {
    var penalty = SpecialActionModifiers.GetPenalty(SpecialActionType.Disarm);
    Assert.AreEqual(-2, penalty);
  }

  [TestMethod]
  public void Disarm_Success_ItemDropped()
  {
    var dice = CreateDice(2);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateDisarm(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6, // TV = 5
      targetItemDescription: "Sword"
    );

    var result = resolver.Resolve(request);

    // AS 10 - 2 penalty = 8, Roll +2 = AV 10, TV 5 → SV 5
    Assert.IsTrue(result.Success);
    Assert.AreEqual(8, result.EffectiveAS);
    Assert.IsTrue(result.ItemDropped);
    Assert.AreEqual("Sword", result.DroppedItemDescription);
  }

  [TestMethod]
  public void Disarm_Miss_ItemNotDropped()
  {
    var dice = CreateDice(-2);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateDisarm(
      attackerAS: 6,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 8, // TV = 7
      targetItemDescription: "Axe"
    );

    var result = resolver.Resolve(request);

    // AS 6 - 2 = 4, Roll -2 = AV 2, TV 7 → SV -5
    Assert.IsFalse(result.Success);
    Assert.IsFalse(result.ItemDropped);
  }

  [TestMethod]
  public void Disarm_Critical_ItemBroken()
  {
    var dice = CreateDice(6);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateDisarm(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 4, // TV = 3
      targetItemDescription: "Dagger"
    );

    var result = resolver.Resolve(request);

    // AS 10 - 2 = 8, Roll +6 = AV 14, TV 3 → SV 11 (critical >= 8)
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.SV >= 8);
    Assert.IsTrue(result.ItemBroken);
    Assert.IsTrue(result.IsCritical);
  }

  [TestMethod]
  public void Disarm_MultipleActionsAppliesPenalty()
  {
    var dice = CreateDice(0);
    var resolver = new SpecialActionResolver(dice);

    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.Disarm,
      AttackerAS = 10,
      DefenderDodgeAS = 6,
      ActionsThisRound = 1, // Already took an action
      TargetItemDescription = "Shield"
    };

    var result = resolver.Resolve(request);

    // AS 10 - 2 (disarm) - 1 (multi-action) = 7
    Assert.AreEqual(7, result.EffectiveAS);
  }

  #endregion

  #region Called Shot Tests

  [TestMethod]
  public void CalledShot_HasMinusTwoASPenalty()
  {
    var penalty = SpecialActionModifiers.GetPenalty(SpecialActionType.CalledShot);
    Assert.AreEqual(-2, penalty);
  }

  [TestMethod]
  public void CalledShot_GoodRoll_HitsIntendedLocation()
  {
    var dice = CreateDice(3, 0); // +3 attack, 0 phys
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateCalledShot(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6, // TV = 5
      targetLocation: HitLocation.Head
    );

    var result = resolver.Resolve(request);

    // AS 10 - 2 = 8, Roll +3 = AV 11, TV 5 → SV 6 (>= 2 hits intended)
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.SV >= 2);
    Assert.IsTrue(result.HitIntendedLocation);
    Assert.AreEqual(HitLocation.Head, result.HitLocation);
  }

  [TestMethod]
  public void CalledShot_MarginalSuccess_HitsRandomLocation()
  {
    // SV 0-1 = marginal success, hits random location
    // The dice roller returns 3 values: attack roll, phys roll, then location roll
    var dice = new DeterministicDiceRoller()
      .Queue4dFPlusResults(-1, 0) // Attack roll and phys roll
      .QueueDiceRolls(5); // Location roll (5 = Torso)
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateCalledShot(
      attackerAS: 8,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6, // TV = 5
      targetLocation: HitLocation.Head
    );

    var result = resolver.Resolve(request);

    // AS 8 - 2 = 6, Roll -1 = AV 5, TV 5 → SV 0 (< 2, random location)
    Assert.IsTrue(result.Success);
    Assert.AreEqual(0, result.SV);
    Assert.IsFalse(result.HitIntendedLocation);
    Assert.AreNotEqual(HitLocation.Head, result.HitLocation);
  }

  [TestMethod]
  public void CalledShot_Miss_NoHit()
  {
    var dice = CreateDice(-5);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateCalledShot(
      attackerAS: 6,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 8, // TV = 7
      targetLocation: HitLocation.Torso
    );

    var result = resolver.Resolve(request);

    // AS 6 - 2 = 4, Roll -5 = AV -1, TV 7 → SV -8
    Assert.IsFalse(result.Success);
    Assert.IsNull(result.HitLocation);
    Assert.IsNull(result.Damage);
  }

  [TestMethod]
  public void CalledShot_AppliesPhysicalityBonus()
  {
    var dice = CreateDice(3, 2); // Attack +3, Phys +2
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateCalledShot(
      attackerAS: 10,
      attackerPhysicalityAS: 10, // PhysAS 10 + roll 2 = 12 vs TV 8 → PhysRV 4
      defenderDodgeAS: 6,
      targetLocation: HitLocation.Torso
    );

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.PhysicalityBonus);
    Assert.IsNotNull(result.FinalSV);
    Assert.AreEqual(result.SV + result.PhysicalityBonus!.SVModifier, result.FinalSV);
  }

  [TestMethod]
  public void CalledShot_IncludesDamageResult()
  {
    var dice = CreateDice(3, 1); // Attack +3, Phys +1
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateCalledShot(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6,
      targetLocation: HitLocation.RightArm
    );

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Damage);
    Assert.IsTrue(result.Damage!.FatigueDamage > 0 || result.Damage.VitalityDamage > 0);
  }

  [TestMethod]
  public void CalledShot_WithoutTargetLocation_Throws()
  {
    var dice = CreateDice(2);
    var resolver = new SpecialActionResolver(dice);

    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.CalledShot,
      AttackerAS = 10,
      DefenderDodgeAS = 6,
      TargetLocation = null // Missing target
    };

    Assert.ThrowsExactly<InvalidOperationException>(() => resolver.Resolve(request));
  }

  [TestMethod]
  public void CalledShot_WithBoosts()
  {
    var dice = CreateDice(0, 0);
    var resolver = new SpecialActionResolver(dice);

    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.CalledShot,
      AttackerAS = 10,
      AttackerPhysicalityAS = 8,
      DefenderDodgeAS = 6,
      TargetLocation = HitLocation.Head,
      APBoost = 2,
      FATBoost = 1
    };

    var result = resolver.Resolve(request);

    // AS 10 - 2 (called shot) + 2 (AP) + 1 (FAT) = 11
    Assert.AreEqual(11, result.EffectiveAS);
  }

  #endregion

  #region Stunning Blow Tests

  [TestMethod]
  public void StunningBlow_HasMinusTwoASPenalty()
  {
    var penalty = SpecialActionModifiers.GetPenalty(SpecialActionType.StunningBlow);
    Assert.AreEqual(-2, penalty);
  }

  [TestMethod]
  public void StunningBlow_Success_StunsDurationEqualsSV()
  {
    var dice = CreateDice(2);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateStunningBlow(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6 // TV = 5
    );

    var result = resolver.Resolve(request);

    // AS 10 - 2 = 8, Roll +2 = AV 10, TV 5 → SV 5
    Assert.IsTrue(result.Success);
    Assert.AreEqual(5, result.SV);
    Assert.AreEqual(5, result.StunDurationSeconds);
    Assert.IsFalse(result.CausedUnconscious);
  }

  [TestMethod]
  public void StunningBlow_SVZero_OneSecondDuration()
  {
    var dice = CreateDice(-1);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateStunningBlow(
      attackerAS: 8,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6 // TV = 5
    );

    var result = resolver.Resolve(request);

    // AS 8 - 2 = 6, Roll -1 = AV 5, TV 5 → SV 0 (minimum hit)
    Assert.IsTrue(result.Success);
    Assert.AreEqual(0, result.SV);
    Assert.AreEqual(1, result.StunDurationSeconds); // Minimum 1 second
  }

  [TestMethod]
  public void StunningBlow_Miss_NoStun()
  {
    var dice = CreateDice(-4);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateStunningBlow(
      attackerAS: 6,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 8 // TV = 7
    );

    var result = resolver.Resolve(request);

    Assert.IsFalse(result.Success);
    Assert.IsNull(result.StunDurationSeconds);
  }

  [TestMethod]
  public void StunningBlow_Critical_CausesUnconscious()
  {
    var dice = CreateDice(7);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateStunningBlow(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 4 // TV = 3
    );

    var result = resolver.Resolve(request);

    // AS 10 - 2 = 8, Roll +7 = AV 15, TV 3 → SV 12 (>= 10 = unconscious)
    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.SV >= 10);
    Assert.IsTrue(result.CausedUnconscious);
    Assert.IsTrue(result.IsCritical);
  }

  [TestMethod]
  public void StunningBlow_NoDamage()
  {
    var dice = CreateDice(3);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateStunningBlow(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6
    );

    var result = resolver.Resolve(request);

    Assert.IsTrue(result.Success);
    Assert.IsNull(result.Damage); // Stun doesn't do damage
  }

  #endregion

  #region Active Defense Tests

  [TestMethod]
  public void SpecialAction_WithActiveDefense_UsesExplicitTV()
  {
    var dice = CreateDice(3);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateDisarm(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6, // Would give passive TV = 5
      targetItemDescription: "Mace"
    );

    // Defender rolled active defense with TV = 10
    var result = resolver.ResolveWithTV(request, tv: 10);

    // AS 10 - 2 = 8, Roll +3 = AV 11, TV 10 → SV 1
    Assert.IsTrue(result.Success);
    Assert.AreEqual(10, result.TV);
    Assert.AreEqual(1, result.SV);
  }

  [TestMethod]
  public void SpecialAction_ActiveDefenseCanCauseMiss()
  {
    var dice = CreateDice(2);
    var resolver = new SpecialActionResolver(dice);

    var request = SpecialActionRequest.CreateKnockback(
      attackerAS: 10,
      attackerPhysicalityAS: 8,
      defenderDodgeAS: 6 // Passive TV = 5
    );

    // Strong active defense
    var result = resolver.ResolveWithTV(request, tv: 15);

    // AS 10, Roll +2 = AV 12, TV 15 → SV -3
    Assert.IsFalse(result.Success);
  }

  #endregion

  #region Request Builder Tests

  [TestMethod]
  public void EffectiveAS_IncludesAllModifiers()
  {
    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.Disarm,
      AttackerAS = 12,
      ActionsThisRound = 1, // -1
      APBoost = 3,
      FATBoost = 2
    };

    // 12 - 2 (disarm) - 1 (multi) + 3 (AP) + 2 (FAT) = 14
    Assert.AreEqual(14, request.GetEffectiveAS());
  }

  [TestMethod]
  public void EffectiveAS_FirstActionNoMultiPenalty()
  {
    var request = new SpecialActionRequest
    {
      ActionType = SpecialActionType.StunningBlow,
      AttackerAS = 10,
      ActionsThisRound = 0 // First action
    };

    // 10 - 2 (stunning blow) = 8
    Assert.AreEqual(8, request.GetEffectiveAS());
  }

  [TestMethod]
  public void PassiveDefenseTV_IsDefenderDodgeMinusOne()
  {
    var request = new SpecialActionRequest
    {
      DefenderDodgeAS = 8
    };

    Assert.AreEqual(7, request.GetPassiveDefenseTV());
  }

  #endregion

  #region Summary Tests

  [TestMethod]
  public void Result_HasDescriptiveSummary()
  {
    var knockback = new SpecialActionResolver(CreateDice(3))
      .Resolve(SpecialActionRequest.CreateKnockback(10, 8, 6));
    Assert.IsTrue(knockback.Summary.Contains("Knockback"));

    var disarm = new SpecialActionResolver(CreateDice(3))
      .Resolve(SpecialActionRequest.CreateDisarm(10, 8, 6, "Sword"));
    Assert.IsTrue(disarm.Summary.Contains("Disarm"));

    var calledShot = new SpecialActionResolver(CreateDice(3, 0))
      .Resolve(SpecialActionRequest.CreateCalledShot(10, 8, 6, HitLocation.Head));
    Assert.IsTrue(calledShot.Summary.Contains("Called Shot"));

    var stun = new SpecialActionResolver(CreateDice(3))
      .Resolve(SpecialActionRequest.CreateStunningBlow(10, 8, 6));
    Assert.IsTrue(stun.Summary.Contains("Stunning Blow"));
  }

  [TestMethod]
  public void Miss_SummaryShowsFailure()
  {
    var dice = CreateDice(-5);
    var resolver = new SpecialActionResolver(dice);

    var result = resolver.Resolve(SpecialActionRequest.CreateKnockback(5, 8, 10));

    Assert.IsTrue(result.Summary.Contains("failed") || result.Summary.Contains("AV"));
  }

  #endregion
}
