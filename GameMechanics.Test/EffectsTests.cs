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
  public void EffectBehaviorFactory_GetBehavior_ReturnsDefaultForUnknown()
  {
    var behavior = EffectBehaviorFactory.GetBehavior(EffectType.Environmental);
    Assert.IsInstanceOfType(behavior, typeof(DefaultEffectBehavior));
  }

  #endregion
}
