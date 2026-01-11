using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class EffectsTests
{
  private ServiceProvider InitServices()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddCsla();
    services.AddMockDb();
    return services.BuildServiceProvider();
  }

  #region WoundState Tests

  [TestMethod]
  public void WoundState_Serialize_RoundTrips()
  {
    var state = new WoundState
    {
      LightWounds = 1,
      SeriousWounds = 2,
      MaxWounds = 4,
      IsCrippled = false,
      IsDestroyed = false,
      RoundsToDamage = 15
    };

    var json = state.Serialize();
    var restored = WoundState.Deserialize(json);

    Assert.AreEqual(state.LightWounds, restored.LightWounds);
    Assert.AreEqual(state.SeriousWounds, restored.SeriousWounds);
    Assert.AreEqual(state.MaxWounds, restored.MaxWounds);
    Assert.AreEqual(state.IsCrippled, restored.IsCrippled);
    Assert.AreEqual(state.IsDestroyed, restored.IsDestroyed);
    Assert.AreEqual(state.RoundsToDamage, restored.RoundsToDamage);
  }

  [TestMethod]
  public void WoundState_TotalWounds_Calculated()
  {
    var state = new WoundState
    {
      LightWounds = 1,
      SeriousWounds = 2
    };

    Assert.AreEqual(3, state.TotalWounds);
  }

  [TestMethod]
  public void WoundState_IsDisabled_WhenNearMax()
  {
    var state = new WoundState
    {
      LightWounds = 0,
      SeriousWounds = 1,
      MaxWounds = 2
    };

    Assert.IsTrue(state.IsDisabled);
  }

  [TestMethod]
  [DataRow("Head", 2)]
  [DataRow("Torso", 4)]
  [DataRow("LeftArm", 2)]
  [DataRow("RightArm", 2)]
  [DataRow("LeftLeg", 2)]
  [DataRow("RightLeg", 2)]
  public void WoundState_GetMaxWoundsForLocation(string location, int expectedMax)
  {
    Assert.AreEqual(expectedMax, WoundState.GetMaxWoundsForLocation(location));
  }

  #endregion

  #region EffectRecord Tests

  [TestMethod]
  public void EffectRecord_Create_SetsDefaults()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var effect = effectPortal.CreateChild(
      EffectType.Wound,
      "Test Wound",
      "LeftArm",
      null,
      null);

    Assert.AreEqual(EffectType.Wound, effect.EffectType);
    Assert.AreEqual("Test Wound", effect.Name);
    Assert.AreEqual("LeftArm", effect.Location);
    Assert.IsNull(effect.DurationRounds);
    Assert.AreEqual(0, effect.ElapsedRounds);
    Assert.AreEqual(1, effect.CurrentStacks);
    Assert.IsTrue(effect.IsActive);
  }

  [TestMethod]
  public void EffectRecord_RemainingRounds_CalculatedCorrectly()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var effect = effectPortal.CreateChild(
      EffectType.Buff,
      "Test Buff",
      null,
      10,
      null);

    Assert.AreEqual(10, effect.RemainingRounds);

    effect.ElapsedRounds = 3;
    Assert.AreEqual(7, effect.RemainingRounds);
  }

  [TestMethod]
  public void EffectRecord_IsExpired_WhenDurationExceeded()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var effect = effectPortal.CreateChild(
      EffectType.Buff,
      "Test Buff",
      null,
      5,
      null);

    Assert.IsFalse(effect.IsExpired);

    effect.ElapsedRounds = 5;
    Assert.IsTrue(effect.IsExpired);
  }

  [TestMethod]
  public void EffectRecord_PermanentEffect_NeverExpires()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var effect = effectPortal.CreateChild(
      EffectType.Wound,
      "Test Wound",
      "Torso",
      null, // null duration = permanent
      null);

    effect.ElapsedRounds = 1000;
    Assert.IsFalse(effect.IsExpired);
    Assert.IsNull(effect.RemainingRounds);
  }

  #endregion

  #region EffectList Tests

  [TestMethod]
  public void EffectList_Create_Empty()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var c = dp.Create(42);

    Assert.IsNotNull(c.Effects);
    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public void EffectList_TotalWoundCount_ZeroWhenNoWounds()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var c = dp.Create(42);

    Assert.AreEqual(0, c.Effects.TotalWoundCount);
  }

  [TestMethod]
  public void EffectList_GetRandomLocation_ReturnsValidLocation()
  {
    var validLocations = new[] { "Head", "Torso", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };

    for (int i = 0; i < 100; i++)
    {
      var location = EffectList.GetRandomLocation();
      Assert.IsTrue(validLocations.Contains(location), $"Invalid location: {location}");
    }
  }

  #endregion

  #region WoundBehavior Tests

  [TestMethod]
  public void WoundBehavior_TakeWound_CreatesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);

    Assert.AreEqual(1, c.Effects.Count);
    Assert.AreEqual(1, c.Effects.TotalWoundCount);
    
    var woundState = c.Effects.GetWoundState("LeftArm");
    Assert.IsNotNull(woundState);
    Assert.AreEqual(1, woundState.SeriousWounds);
    Assert.AreEqual(0, woundState.LightWounds);
  }

  [TestMethod]
  public void WoundBehavior_TakeWound_StacksAtSameLocation()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);

    // Should still be one effect, but with 2 serious wounds
    Assert.AreEqual(1, c.Effects.Count);
    Assert.AreEqual(2, c.Effects.TotalWoundCount);
    
    var woundState = c.Effects.GetWoundState("LeftArm");
    Assert.IsNotNull(woundState);
    Assert.AreEqual(2, woundState.SeriousWounds);
  }

  [TestMethod]
  public void WoundBehavior_TakeWound_DifferentLocations_SeparateEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.TakeWound(c, "RightArm", effectPortal);

    Assert.AreEqual(2, c.Effects.Count);
    Assert.AreEqual(2, c.Effects.TotalWoundCount);
  }

  [TestMethod]
  public void WoundBehavior_TakeWound_SetsCrippled()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    // LeftArm has MaxWounds = 2
    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);

    var woundState = c.Effects.GetWoundState("LeftArm");
    Assert.IsNotNull(woundState);
    Assert.IsTrue(woundState.IsCrippled);
  }

  [TestMethod]
  public void WoundBehavior_HealWound_SeriousToLight()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.HealWound(c, "LeftArm");

    var woundState = c.Effects.GetWoundState("LeftArm");
    Assert.IsNotNull(woundState);
    Assert.AreEqual(0, woundState.SeriousWounds);
    Assert.AreEqual(1, woundState.LightWounds);
  }

  [TestMethod]
  public void WoundBehavior_HealWound_RemovesEffectWhenFullyHealed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.HealWound(c, "LeftArm"); // Serious -> Light
    WoundBehavior.HealWound(c, "LeftArm"); // Light -> removed

    Assert.AreEqual(0, c.Effects.Count);
    Assert.AreEqual(0, c.Effects.TotalWoundCount);
  }

  [TestMethod]
  public void WoundBehavior_GetAbilityScoreModifiers_AppliesPenalty()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);
    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);

    // 2 wounds = -4 penalty
    var modifier = c.Effects.GetAbilityScoreModifier("Melee Combat", "DEX", 10);
    Assert.AreEqual(-4, modifier);
  }

  [TestMethod]
  public void WoundBehavior_OnTick_AppliesPeriodicDamage()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    WoundBehavior.TakeWound(c, "LeftArm", effectPortal);

    // Get initial state
    var woundState = c.Effects.GetWoundState("LeftArm");
    Assert.IsNotNull(woundState);
    Assert.AreEqual(20, woundState.RoundsToDamage);

    // Run 19 rounds - no damage yet
    for (int i = 0; i < 19; i++)
    {
      c.Effects.EndOfRound();
    }

    Assert.AreEqual(0, c.Vitality.PendingDamage);
    Assert.AreEqual(0, c.Fatigue.PendingDamage);

    // Round 20 should apply damage
    c.Effects.EndOfRound();

    // 1 serious wound = 1 VIT damage, 2 FAT damage
    Assert.AreEqual(1, c.Vitality.PendingDamage);
    Assert.AreEqual(2, c.Fatigue.PendingDamage);
  }

  #endregion

  #region CharacterEdit Integration Tests

  [TestMethod]
  public void Character_GetEffectiveAttribute_IncludesEffectModifiers()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var c = dp.Create(42);

    // Get base STR
    var baseStr = c.GetAttribute("STR");

    // No wounds, effective should equal base
    Assert.AreEqual(baseStr, c.GetEffectiveAttribute("STR"));

    // Note: We'd need to add an attribute-modifying effect to fully test this
    // For now, wounds only affect AS, not attributes directly
  }

  [TestMethod]
  public void Character_TakeDamage_CreatesWounds()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    // Damage value of 7-9 = 1 wound
    var rv = Reference.ResultValues.GetResult(5);
    Reference.DamageValue dmg;
    do
    {
      dmg = rv.CalculateDamageValue(7, 1);
    } while (dmg.Damage < 7 || dmg.Damage > 9);

    c.TakeDamage(dmg, effectPortal);

    Assert.AreEqual(1, c.Effects.TotalWoundCount);
  }

  [TestMethod]
  public void Character_TakeDamage_HighDamageCreatesMultipleWounds()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    // Damage value of 10-14 = 2 wounds
    var rv = Reference.ResultValues.GetResult(8);
    Reference.DamageValue dmg;
    do
    {
      dmg = rv.CalculateDamageValue(10, 1);
    } while (dmg.Damage < 10 || dmg.Damage > 14);

    c.TakeDamage(dmg, effectPortal);

    Assert.AreEqual(2, c.Effects.TotalWoundCount);
  }

  #endregion

  #region EffectAddResult Tests

  [TestMethod]
  public void EffectAddResult_AddNormally_CorrectOutcome()
  {
    var result = EffectAddResult.AddNormally();
    Assert.AreEqual(EffectAddOutcome.Add, result.Outcome);
  }

  [TestMethod]
  public void EffectAddResult_Reject_CorrectOutcome()
  {
    var result = EffectAddResult.Reject("Test reason");
    Assert.AreEqual(EffectAddOutcome.Reject, result.Outcome);
    Assert.AreEqual("Test reason", result.Message);
  }

  [TestMethod]
  public void EffectAddResult_Replace_CorrectOutcome()
  {
    var id = System.Guid.NewGuid();
    var result = EffectAddResult.Replace(id, "Replacing existing");
    Assert.AreEqual(EffectAddOutcome.Replace, result.Outcome);
    Assert.AreEqual(id, result.ReplaceEffectId);
  }

  #endregion

  #region EffectBehaviorFactory Tests

  [TestMethod]
  public void EffectBehaviorFactory_GetBehavior_ReturnsWoundBehavior()
  {
    var behavior = EffectBehaviorFactory.GetBehavior(EffectType.Wound);
    Assert.IsInstanceOfType(behavior, typeof(WoundBehavior));
  }

  [TestMethod]
  public void EffectBehaviorFactory_GetBehavior_ReturnsPoisonBehavior()
  {
    var behavior = EffectBehaviorFactory.GetBehavior(EffectType.Poison);
    Assert.IsInstanceOfType(behavior, typeof(PoisonBehavior));
  }

  [TestMethod]
  public void EffectBehaviorFactory_GetBehavior_ReturnsDefaultForUnknown()
  {
    var behavior = EffectBehaviorFactory.GetBehavior(EffectType.Environmental);
    Assert.IsInstanceOfType(behavior, typeof(DefaultEffectBehavior));
  }

  #endregion

  #region PoisonState Tests

  [TestMethod]
  public void PoisonState_Serialize_RoundTrips()
  {
    var state = new PoisonState
    {
      PoisonName = "Test Poison",
      DamageType = PoisonDamageType.Combined,
      BaseFatigueDamage = 3,
      BaseVitalityDamage = 2,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 5,
      TotalDurationRounds = 100,
      ElapsedRounds = 20,
      BaseASPenalty = -3,
      CanCreateWounds = true,
      WoundThreshold = 0.3,
      Stacks = 2,
      MaxStacks = 5
    };

    var json = state.Serialize();
    var restored = PoisonState.Deserialize(json);

    Assert.AreEqual(state.PoisonName, restored.PoisonName);
    Assert.AreEqual(state.DamageType, restored.DamageType);
    Assert.AreEqual(state.BaseFatigueDamage, restored.BaseFatigueDamage);
    Assert.AreEqual(state.BaseVitalityDamage, restored.BaseVitalityDamage);
    Assert.AreEqual(state.TickIntervalRounds, restored.TickIntervalRounds);
    Assert.AreEqual(state.RoundsUntilNextTick, restored.RoundsUntilNextTick);
    Assert.AreEqual(state.TotalDurationRounds, restored.TotalDurationRounds);
    Assert.AreEqual(state.ElapsedRounds, restored.ElapsedRounds);
    Assert.AreEqual(state.BaseASPenalty, restored.BaseASPenalty);
    Assert.AreEqual(state.CanCreateWounds, restored.CanCreateWounds);
    Assert.AreEqual(state.WoundThreshold, restored.WoundThreshold);
    Assert.AreEqual(state.Stacks, restored.Stacks);
    Assert.AreEqual(state.MaxStacks, restored.MaxStacks);
  }

  [TestMethod]
  public void PoisonState_EffectivenessMultiplier_DecreasesOverTime()
  {
    var state = new PoisonState
    {
      TotalDurationRounds = 100,
      ElapsedRounds = 0
    };

    // At start, effectiveness is 100%
    Assert.AreEqual(1.0, state.EffectivenessMultiplier);

    // At 50% elapsed, effectiveness is 50%
    state.ElapsedRounds = 50;
    Assert.AreEqual(0.5, state.EffectivenessMultiplier);

    // At 90% elapsed, effectiveness is 10%
    state.ElapsedRounds = 90;
    Assert.AreEqual(0.1, state.EffectivenessMultiplier, 0.001);

    // At 100% elapsed, effectiveness is 0%
    state.ElapsedRounds = 100;
    Assert.AreEqual(0.0, state.EffectivenessMultiplier);
  }

  [TestMethod]
  public void PoisonState_CurrentDamage_ScalesWithEffectiveness()
  {
    var state = new PoisonState
    {
      BaseFatigueDamage = 4,
      BaseVitalityDamage = 2,
      TotalDurationRounds = 100,
      ElapsedRounds = 0,
      Stacks = 1
    };

    // At start, full damage
    Assert.AreEqual(4, state.CurrentFatigueDamage);
    Assert.AreEqual(2, state.CurrentVitalityDamage);

    // At 50% elapsed, half damage (ceiling)
    state.ElapsedRounds = 50;
    Assert.AreEqual(2, state.CurrentFatigueDamage); // ceil(4 * 0.5) = 2
    Assert.AreEqual(1, state.CurrentVitalityDamage); // ceil(2 * 0.5) = 1

    // Near end, minimal damage
    state.ElapsedRounds = 95;
    Assert.AreEqual(1, state.CurrentFatigueDamage); // ceil(4 * 0.05) = 1
    Assert.AreEqual(1, state.CurrentVitalityDamage); // ceil(2 * 0.05) = 1
  }

  [TestMethod]
  public void PoisonState_CurrentDamage_ScalesWithStacks()
  {
    var state = new PoisonState
    {
      BaseFatigueDamage = 2,
      BaseVitalityDamage = 1,
      TotalDurationRounds = 100,
      ElapsedRounds = 0,
      Stacks = 3
    };

    // Stacks multiply base damage
    Assert.AreEqual(6, state.CurrentFatigueDamage); // 2 * 3
    Assert.AreEqual(3, state.CurrentVitalityDamage); // 1 * 3
  }

  [TestMethod]
  public void PoisonState_CurrentASPenalty_DiminishesOverTime()
  {
    var state = new PoisonState
    {
      BaseASPenalty = -4,
      TotalDurationRounds = 100,
      ElapsedRounds = 0,
      Stacks = 1
    };

    // At start, full penalty
    Assert.AreEqual(-4, state.CurrentASPenalty);

    // At 50% elapsed, half penalty
    state.ElapsedRounds = 50;
    Assert.AreEqual(-2, state.CurrentASPenalty); // ceil(4 * 0.5) = 2, negated = -2

    // Near end, minimal penalty
    state.ElapsedRounds = 90;
    Assert.AreEqual(-1, state.CurrentASPenalty);
  }

  [TestMethod]
  public void PoisonState_IsInWoundPhase_TrueWhenFresh()
  {
    var state = new PoisonState
    {
      CanCreateWounds = true,
      WoundThreshold = 0.25,
      TotalDurationRounds = 100,
      ElapsedRounds = 0
    };

    // At start (effectiveness 1.0), should be in wound phase
    Assert.IsTrue(state.IsInWoundPhase);

    // At 20% elapsed (effectiveness 0.8), still in wound phase
    state.ElapsedRounds = 20;
    Assert.IsTrue(state.IsInWoundPhase);

    // At 30% elapsed (effectiveness 0.7), past wound phase
    state.ElapsedRounds = 30;
    Assert.IsFalse(state.IsInWoundPhase);
  }

  [TestMethod]
  public void PoisonState_CreateWeakPoison_HasCorrectDefaults()
  {
    var poison = PoisonState.CreateWeakPoison();

    Assert.AreEqual("Weak Poison", poison.PoisonName);
    Assert.AreEqual(PoisonDamageType.FatigueOnly, poison.DamageType);
    Assert.AreEqual(2, poison.BaseFatigueDamage);
    Assert.AreEqual(0, poison.BaseVitalityDamage);
    Assert.IsFalse(poison.CanCreateWounds);
  }

  [TestMethod]
  public void PoisonState_CreateStrongPoison_HasCorrectDefaults()
  {
    var poison = PoisonState.CreateStrongPoison();

    Assert.AreEqual("Strong Poison", poison.PoisonName);
    Assert.AreEqual(PoisonDamageType.VitalityOnly, poison.DamageType);
    Assert.AreEqual(0, poison.BaseFatigueDamage);
    Assert.AreEqual(2, poison.BaseVitalityDamage);
    Assert.IsTrue(poison.CanCreateWounds);
  }

  [TestMethod]
  public void PoisonState_CreateDeadlyPoison_HasCorrectDefaults()
  {
    var poison = PoisonState.CreateDeadlyPoison();

    Assert.AreEqual("Deadly Poison", poison.PoisonName);
    Assert.AreEqual(PoisonDamageType.Combined, poison.DamageType);
    Assert.IsTrue(poison.BaseFatigueDamage > 0);
    Assert.IsTrue(poison.BaseVitalityDamage > 0);
    Assert.IsTrue(poison.CanCreateWounds);
  }

  [TestMethod]
  public void PoisonState_CreateSleepPoison_HasCorrectDefaults()
  {
    var poison = PoisonState.CreateSleepPoison();

    Assert.AreEqual("Sleep Poison", poison.PoisonName);
    Assert.AreEqual(PoisonDamageType.FatigueOnly, poison.DamageType);
    Assert.IsTrue(poison.BaseFatigueDamage > 0);
    Assert.AreEqual(0, poison.BaseVitalityDamage);
    Assert.IsFalse(poison.CanCreateWounds);
  }

  #endregion

  #region PoisonBehavior Tests

  [TestMethod]
  public void PoisonBehavior_ApplyPoison_CreatesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = PoisonState.CreateWeakPoison();
    c.Effects.ApplyPoison(poison, effectPortal);

    Assert.AreEqual(1, c.Effects.Count);
    Assert.IsTrue(c.Effects.IsPoisoned);
  }

  [TestMethod]
  public void PoisonBehavior_ApplyPoison_SameNameStacks()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison1 = PoisonState.CreateWeakPoison();
    var poison2 = PoisonState.CreateWeakPoison();

    c.Effects.ApplyPoison(poison1, effectPortal);
    c.Effects.ApplyPoison(poison2, effectPortal);

    // Should still be one effect, but stacked
    Assert.AreEqual(1, c.Effects.Count);

    var activePoisons = c.Effects.GetActivePoisons().ToList();
    Assert.AreEqual(1, activePoisons.Count);
    Assert.AreEqual(2, activePoisons[0].State.Stacks);
  }

  [TestMethod]
  public void PoisonBehavior_ApplyPoison_DifferentNamesDoNotStack()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var weak = PoisonState.CreateWeakPoison();
    var strong = PoisonState.CreateStrongPoison();

    c.Effects.ApplyPoison(weak, effectPortal);
    c.Effects.ApplyPoison(strong, effectPortal);

    Assert.AreEqual(2, c.Effects.Count);
  }

  [TestMethod]
  public void PoisonBehavior_OnTick_DealsFatigueDamage()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    // Create a poison with short tick interval
    var poison = new PoisonState
    {
      PoisonName = "Quick Test Poison",
      DamageType = PoisonDamageType.FatigueOnly,
      BaseFatigueDamage = 3,
      TickIntervalRounds = 5,
      RoundsUntilNextTick = 5,
      TotalDurationRounds = 50
    };

    c.Effects.ApplyPoison(poison, effectPortal);

    var initialFat = c.Fatigue.PendingDamage;

    // Run 4 rounds - no damage yet
    for (int i = 0; i < 4; i++)
    {
      c.Effects.EndOfRound();
    }
    Assert.AreEqual(initialFat, c.Fatigue.PendingDamage, "No damage before tick");

    // 5th round triggers damage
    c.Effects.EndOfRound();
    Assert.IsTrue(c.Fatigue.PendingDamage > initialFat, "Damage should be dealt on tick");
  }

  [TestMethod]
  public void PoisonBehavior_OnTick_DealsVitalityDamage()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "VIT Test Poison",
      DamageType = PoisonDamageType.VitalityOnly,
      BaseVitalityDamage = 2,
      TickIntervalRounds = 3,
      RoundsUntilNextTick = 3,
      TotalDurationRounds = 30
    };

    c.Effects.ApplyPoison(poison, effectPortal);

    var initialVit = c.Vitality.PendingDamage;

    // Run to tick
    for (int i = 0; i < 3; i++)
    {
      c.Effects.EndOfRound();
    }

    Assert.IsTrue(c.Vitality.PendingDamage > initialVit, "VIT damage should be dealt");
  }

  [TestMethod]
  public void PoisonBehavior_OnTick_DealsCombinedDamage()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "Combined Test Poison",
      DamageType = PoisonDamageType.Combined,
      BaseFatigueDamage = 2,
      BaseVitalityDamage = 1,
      TickIntervalRounds = 2,
      RoundsUntilNextTick = 2,
      TotalDurationRounds = 20
    };

    c.Effects.ApplyPoison(poison, effectPortal);

    var initialFat = c.Fatigue.PendingDamage;
    var initialVit = c.Vitality.PendingDamage;

    // Run to tick
    c.Effects.EndOfRound();
    c.Effects.EndOfRound();

    Assert.IsTrue(c.Fatigue.PendingDamage > initialFat, "FAT damage should be dealt");
    Assert.IsTrue(c.Vitality.PendingDamage > initialVit, "VIT damage should be dealt");
  }

  [TestMethod]
  public void PoisonBehavior_GetAbilityScoreModifiers_AppliesPenalty()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "Penalty Test Poison",
      BaseASPenalty = -3,
      TotalDurationRounds = 100,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10
    };

    c.Effects.ApplyPoison(poison, effectPortal);

    var modifier = c.Effects.GetAbilityScoreModifier("Any Skill", "DEX", 10);
    Assert.AreEqual(-3, modifier);
  }

  [TestMethod]
  public void PoisonBehavior_PenaltyDiminishesOverTime()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "Diminishing Test",
      BaseASPenalty = -4,
      TotalDurationRounds = 100,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10
    };

    c.Effects.ApplyPoison(poison, effectPortal);

    // Initial penalty
    var initialPenalty = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);
    Assert.AreEqual(-4, initialPenalty);

    // Advance 50 rounds (50% through)
    for (int i = 0; i < 50; i++)
    {
      c.Effects.EndOfRound();
    }

    var halfwayPenalty = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);
    Assert.AreEqual(-2, halfwayPenalty); // ceil(4 * 0.5) = 2
  }

  [TestMethod]
  public void PoisonBehavior_ExpiresAfterDuration()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "Short Poison",
      TotalDurationRounds = 10,
      TickIntervalRounds = 5,
      RoundsUntilNextTick = 5
    };

    c.Effects.ApplyPoison(poison, effectPortal);
    Assert.IsTrue(c.Effects.IsPoisoned);

    // Run through duration
    for (int i = 0; i < 10; i++)
    {
      c.Effects.EndOfRound();
    }

    Assert.IsFalse(c.Effects.IsPoisoned, "Poison should have expired");
  }

  [TestMethod]
  public void PoisonBehavior_StackingIncreasesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison = new PoisonState
    {
      PoisonName = "Stackable Poison",
      BaseASPenalty = -2,
      TotalDurationRounds = 100,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10,
      MaxStacks = 3
    };

    // Apply once
    c.Effects.ApplyPoison(poison, effectPortal);
    var penalty1 = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);

    // Apply again (stacks)
    c.Effects.ApplyPoison(poison, effectPortal);
    var penalty2 = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);

    // Penalty should be worse with stacks
    Assert.IsTrue(penalty2 < penalty1, $"Stacked penalty {penalty2} should be worse than {penalty1}");
  }

  [TestMethod]
  public void PoisonBehavior_TotalPoisonPenalty_SumsAllPoisons()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var poison1 = new PoisonState
    {
      PoisonName = "Poison A",
      BaseASPenalty = -2,
      TotalDurationRounds = 100,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10
    };

    var poison2 = new PoisonState
    {
      PoisonName = "Poison B",
      BaseASPenalty = -3,
      TotalDurationRounds = 100,
      TickIntervalRounds = 10,
      RoundsUntilNextTick = 10
    };

    c.Effects.ApplyPoison(poison1, effectPortal);
    c.Effects.ApplyPoison(poison2, effectPortal);

    // Total should sum both penalties
    Assert.AreEqual(-5, c.Effects.TotalPoisonPenalty);
  }

  #endregion

  #region SpellBuffState Tests

  [TestMethod]
  public void SpellBuffState_Serialize_RoundTrips()
  {
    var state = new SpellBuffState
    {
      BuffName = "Test Buff",
      Description = "A test buff",
      Source = "Test Spell",
      TotalDurationRounds = 50,
      ElapsedRounds = 10,
      DiminishesOverTime = true,
      CasterLevel = 5,
      Modifiers =
      [
        new BuffModifier { Type = BuffModifierType.Attribute, Target = "STR", Value = 2 },
        new BuffModifier { Type = BuffModifierType.AbilityScoreGlobal, Target = "All", Value = 1 }
      ]
    };

    var json = state.Serialize();
    var restored = SpellBuffState.Deserialize(json);

    Assert.AreEqual(state.BuffName, restored.BuffName);
    Assert.AreEqual(state.Description, restored.Description);
    Assert.AreEqual(state.Source, restored.Source);
    Assert.AreEqual(state.TotalDurationRounds, restored.TotalDurationRounds);
    Assert.AreEqual(state.ElapsedRounds, restored.ElapsedRounds);
    Assert.AreEqual(state.DiminishesOverTime, restored.DiminishesOverTime);
    Assert.AreEqual(state.CasterLevel, restored.CasterLevel);
    Assert.AreEqual(2, restored.Modifiers.Count);
  }

  [TestMethod]
  public void SpellBuffState_EffectivenessMultiplier_ConstantWhenNotDiminishing()
  {
    var state = new SpellBuffState
    {
      TotalDurationRounds = 100,
      ElapsedRounds = 50,
      DiminishesOverTime = false
    };

    Assert.AreEqual(1.0, state.EffectivenessMultiplier);
  }

  [TestMethod]
  public void SpellBuffState_EffectivenessMultiplier_DiminishesOverTime()
  {
    var state = new SpellBuffState
    {
      TotalDurationRounds = 100,
      ElapsedRounds = 0,
      DiminishesOverTime = true
    };

    Assert.AreEqual(1.0, state.EffectivenessMultiplier);

    state.ElapsedRounds = 50;
    Assert.AreEqual(0.5, state.EffectivenessMultiplier);

    state.ElapsedRounds = 90;
    Assert.AreEqual(0.1, state.EffectivenessMultiplier, 0.001);
  }

  [TestMethod]
  public void SpellBuffState_GetEffectiveValue_ConstantWhenNotDiminishing()
  {
    var state = new SpellBuffState
    {
      TotalDurationRounds = 100,
      ElapsedRounds = 50,
      DiminishesOverTime = false
    };

    var modifier = new BuffModifier { Value = 4 };
    Assert.AreEqual(4, state.GetEffectiveValue(modifier));
  }

  [TestMethod]
  public void SpellBuffState_GetEffectiveValue_ScalesWhenDiminishing()
  {
    var state = new SpellBuffState
    {
      TotalDurationRounds = 100,
      ElapsedRounds = 50,
      DiminishesOverTime = true
    };

    var modifier = new BuffModifier { Value = 4 };
    Assert.AreEqual(2, state.GetEffectiveValue(modifier)); // ceil(4 * 0.5) = 2
  }

  [TestMethod]
  public void SpellBuffState_CreateAttributeBuff_HasCorrectModifier()
  {
    var buff = SpellBuffState.CreateAttributeBuff("Strength", "STR", 3, 60);

    Assert.AreEqual("Strength", buff.BuffName);
    Assert.AreEqual(60, buff.TotalDurationRounds);
    Assert.AreEqual(1, buff.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.Attribute, buff.Modifiers[0].Type);
    Assert.AreEqual("STR", buff.Modifiers[0].Target);
    Assert.AreEqual(3, buff.Modifiers[0].Value);
  }

  [TestMethod]
  public void SpellBuffState_CreateGlobalASBuff_HasCorrectModifier()
  {
    var buff = SpellBuffState.CreateGlobalASBuff("Heroism", 2, 100);

    Assert.AreEqual(1, buff.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.AbilityScoreGlobal, buff.Modifiers[0].Type);
    Assert.AreEqual(2, buff.Modifiers[0].Value);
  }

  [TestMethod]
  public void SpellBuffState_CreateSkillBuff_HasCorrectModifier()
  {
    var buff = SpellBuffState.CreateSkillBuff("Cat's Grace", "Dodge", 3, 60);

    Assert.AreEqual(1, buff.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.AbilityScoreSkill, buff.Modifiers[0].Type);
    Assert.AreEqual("Dodge", buff.Modifiers[0].Target);
    Assert.AreEqual(3, buff.Modifiers[0].Value);
  }

  [TestMethod]
  public void SpellBuffState_CreateHealingBuff_HasCorrectModifiers()
  {
    var buff = SpellBuffState.CreateHealingBuff("Regeneration", 2, 5, 60, healsFatigue: true, healsVitality: true);

    Assert.AreEqual(2, buff.Modifiers.Count);
    Assert.IsTrue(buff.Modifiers.Any(m => m.Target == "FAT" && m.Value == 2));
    Assert.IsTrue(buff.Modifiers.Any(m => m.Target == "VIT" && m.Value == 2));
  }

  [TestMethod]
  public void SpellBuffState_CreateCombatBuff_HasMultipleModifiers()
  {
    var buff = SpellBuffState.CreateCombatBuff("Battle Fury", strBonus: 2, dexBonus: 1, asBonus: 1, durationRounds: 30);

    Assert.AreEqual(3, buff.Modifiers.Count);
    Assert.IsTrue(buff.Modifiers.Any(m => m.Type == BuffModifierType.Attribute && m.Target == "STR"));
    Assert.IsTrue(buff.Modifiers.Any(m => m.Type == BuffModifierType.Attribute && m.Target == "DEX"));
    Assert.IsTrue(buff.Modifiers.Any(m => m.Type == BuffModifierType.AbilityScoreGlobal));
  }

  #endregion

  #region SpellBuffBehavior Tests

  [TestMethod]
  public void SpellBuffBehavior_ApplyBuff_CreatesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateGlobalASBuff("Test Buff", 2, 60);
    var result = c.Effects.ApplySpellBuff(buff, effectPortal);

    Assert.IsTrue(result, "Buff should be applied");
    Assert.AreEqual(1, c.Effects.Count);
    Assert.IsTrue(c.Effects.HasBuff("Test Buff"));
  }

  [TestMethod]
  public void SpellBuffBehavior_ApplyBuff_SameNameFails()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff1 = SpellBuffState.CreateGlobalASBuff("Heroism", 2, 60);
    var buff2 = SpellBuffState.CreateGlobalASBuff("Heroism", 3, 120);

    var result1 = c.Effects.ApplySpellBuff(buff1, effectPortal);
    var result2 = c.Effects.ApplySpellBuff(buff2, effectPortal);

    Assert.IsTrue(result1, "First buff should apply");
    Assert.IsFalse(result2, "Second buff with same name should fail");
    Assert.AreEqual(1, c.Effects.Count);
  }

  [TestMethod]
  public void SpellBuffBehavior_ApplyBuff_DifferentNamesSucceed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff1 = SpellBuffState.CreateGlobalASBuff("Heroism", 2, 60);
    var buff2 = SpellBuffState.CreateAttributeBuff("Bull's Strength", "STR", 3, 60);

    c.Effects.ApplySpellBuff(buff1, effectPortal);
    c.Effects.ApplySpellBuff(buff2, effectPortal);

    Assert.AreEqual(2, c.Effects.Count);
    Assert.IsTrue(c.Effects.HasBuff("Heroism"));
    Assert.IsTrue(c.Effects.HasBuff("Bull's Strength"));
  }

  [TestMethod]
  public void SpellBuffBehavior_GetAttributeModifiers_AppliesBonus()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateAttributeBuff("Bull's Strength", "STR", 3, 60);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    var modifier = c.Effects.GetAttributeModifier("STR", 10);
    Assert.AreEqual(3, modifier);

    // Other attributes unaffected
    var dexModifier = c.Effects.GetAttributeModifier("DEX", 10);
    Assert.AreEqual(0, dexModifier);
  }

  [TestMethod]
  public void SpellBuffBehavior_GetAbilityScoreModifiers_GlobalApplies()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateGlobalASBuff("Heroism", 2, 60);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    var modifier = c.Effects.GetAbilityScoreModifier("Any Skill", "DEX", 10);
    Assert.AreEqual(2, modifier);
  }

  [TestMethod]
  public void SpellBuffBehavior_GetAbilityScoreModifiers_SkillSpecificApplies()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateSkillBuff("Cat's Grace", "Dodge", 3, 60);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    // Dodge should get bonus
    var dodgeModifier = c.Effects.GetAbilityScoreModifier("Dodge", "DEX", 10);
    Assert.AreEqual(3, dodgeModifier);

    // Other skills should not
    var meleeModifier = c.Effects.GetAbilityScoreModifier("Melee", "STR", 10);
    Assert.AreEqual(0, meleeModifier);
  }

  [TestMethod]
  public void SpellBuffBehavior_OnTick_HealingAppliesAtInterval()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateHealingBuff("Regeneration", healPerTick: 3, tickIntervalRounds: 5, durationRounds: 60);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    var initialHeal = c.Fatigue.PendingHealing;

    // Run 4 rounds - no healing yet
    for (int i = 0; i < 4; i++)
    {
      c.Effects.EndOfRound();
    }
    Assert.AreEqual(initialHeal, c.Fatigue.PendingHealing);

    // 5th round triggers healing
    c.Effects.EndOfRound();
    Assert.AreEqual(initialHeal + 3, c.Fatigue.PendingHealing);
  }

  [TestMethod]
  public void SpellBuffBehavior_ExpiresAfterDuration()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateGlobalASBuff("Short Buff", 2, 10);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    Assert.IsTrue(c.Effects.HasBuff("Short Buff"));

    // Run through duration
    for (int i = 0; i < 10; i++)
    {
      c.Effects.EndOfRound();
    }

    Assert.IsFalse(c.Effects.HasBuff("Short Buff"), "Buff should have expired");
  }

  [TestMethod]
  public void SpellBuffBehavior_DiminishingBuff_ReducesOverTime()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateGlobalASBuff("Fading Heroism", 4, 100, diminishes: true);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    // Initial bonus
    var initialModifier = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);
    Assert.AreEqual(4, initialModifier);

    // Advance 50 rounds
    for (int i = 0; i < 50; i++)
    {
      c.Effects.EndOfRound();
    }

    var halfwayModifier = c.Effects.GetAbilityScoreModifier("Any", "STR", 10);
    Assert.AreEqual(2, halfwayModifier); // ceil(4 * 0.5) = 2
  }

  [TestMethod]
  public void SpellBuffBehavior_TryDispel_SucceedsWithSufficientPower()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = new SpellBuffState
    {
      BuffName = "Protected Buff",
      CasterLevel = 5,
      TotalDurationRounds = 100,
      Modifiers = [new BuffModifier { Type = BuffModifierType.AbilityScoreGlobal, Value = 2 }]
    };
    c.Effects.ApplySpellBuff(buff, effectPortal);

    // Dispel with insufficient power fails
    var failedDispel = c.Effects.TryDispelBuff("Protected Buff", 4);
    Assert.IsFalse(failedDispel);
    Assert.IsTrue(c.Effects.HasBuff("Protected Buff"));

    // Dispel with sufficient power succeeds
    var successDispel = c.Effects.TryDispelBuff("Protected Buff", 5);
    Assert.IsTrue(successDispel);
    Assert.IsFalse(c.Effects.HasBuff("Protected Buff"));
  }

  [TestMethod]
  public void SpellBuffBehavior_GetActiveBuffs_ReturnsAllBuffs()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    c.Effects.ApplySpellBuff(SpellBuffState.CreateGlobalASBuff("Buff A", 1, 60), effectPortal);
    c.Effects.ApplySpellBuff(SpellBuffState.CreateAttributeBuff("Buff B", "STR", 2, 60), effectPortal);

    var activeBuffs = c.Effects.GetActiveBuffs().ToList();
    Assert.AreEqual(2, activeBuffs.Count);
    Assert.IsTrue(activeBuffs.Any(b => b.Name == "Buff A"));
    Assert.IsTrue(activeBuffs.Any(b => b.Name == "Buff B"));
  }

  [TestMethod]
  public void SpellBuffBehavior_MultipleModifiers_AllApply()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var buff = SpellBuffState.CreateCombatBuff("Battle Fury", strBonus: 2, dexBonus: 1, asBonus: 1, durationRounds: 60);
    c.Effects.ApplySpellBuff(buff, effectPortal);

    // Attribute modifiers
    Assert.AreEqual(2, c.Effects.GetAttributeModifier("STR", 10));
    Assert.AreEqual(1, c.Effects.GetAttributeModifier("DEX", 10));

    // AS modifier
    Assert.AreEqual(1, c.Effects.GetAbilityScoreModifier("Any", "STR", 10));
  }

  #endregion
}


