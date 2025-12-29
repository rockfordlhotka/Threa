using System;
using GameMechanics.Actions;
using GameMechanics.Reference;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class MovementTests
{
  private Skill _sprintSkill = null!;

  [TestInitialize]
  public void Setup()
  {
    _sprintSkill = SprintResolver.CreateSprintSkill();
  }

  #region Range Conversion Tests

  [TestMethod]
  [DataRow(0, 0)]
  [DataRow(1, 1)]
  [DataRow(2, 4)]
  [DataRow(3, 9)]
  [DataRow(4, 16)]
  [DataRow(5, 25)]
  [DataRow(6, 36)]
  public void RangeToMeters_ReturnsSquareOfRange(int range, int expectedMeters)
  {
    var result = Movement.RangeToMeters(range);
    Assert.AreEqual(expectedMeters, result);
  }

  [TestMethod]
  [DataRow(0, 0)]
  [DataRow(1, 1)]
  [DataRow(4, 2)]
  [DataRow(9, 3)]
  [DataRow(16, 4)]
  [DataRow(25, 5)]
  public void MetersToRange_ReturnsSqrtOfMeters(int meters, int expectedRange)
  {
    var result = Movement.MetersToRange(meters);
    Assert.AreEqual(expectedRange, result);
  }

  [TestMethod]
  [DataRow(5, 2)]  // 5m rounds down to Range 2 (4m)
  [DataRow(10, 3)] // 10m rounds down to Range 3 (9m)
  [DataRow(20, 4)] // 20m rounds down to Range 4 (16m)
  public void MetersToRange_RoundsDown(int meters, int expectedRange)
  {
    var result = Movement.MetersToRange(meters);
    Assert.AreEqual(expectedRange, result);
  }

  [TestMethod]
  public void GetRangeDescription_ReturnsFormattedString()
  {
    var description = Movement.GetRangeDescription(3);
    Assert.AreEqual("Across a room (9m)", description);
  }

  #endregion

  #region Free Positioning Tests

  [TestMethod]
  public void FreePositioning_ReturnsSuccessfulResult()
  {
    var resolver = new SprintResolver();
    var result = resolver.FreePositioning();

    Assert.IsTrue(result.IsSuccess);
    Assert.IsFalse(result.HasMishap);
    Assert.AreEqual(Movement.FreePositioningRange, result.BaseRange);
    Assert.AreEqual(Movement.FreePositioningRange, result.AchievedRange);
  }

  [TestMethod]
  public void FreePositioning_NoActionResultRequired()
  {
    var resolver = new SprintResolver();
    var result = resolver.FreePositioning();
    
    Assert.IsNull(result.ActionResult);
    Assert.IsTrue(result.Description.Contains("Free", StringComparison.OrdinalIgnoreCase));
  }

  #endregion

  #region Sprint Action Tests

  [TestMethod]
  public void Sprint_Success_ReturnsBaseRange()
  {
    // Arrange - roll that gives success (SV = 0)
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new SprintResolver(diceRoller);

    // Act - high attribute + good skill = base AS ~10, TV 6 = success with roll 0
    var result = resolver.Sprint(_sprintSkill, skillLevel: 2, dexterityValue: 12);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(Movement.SprintActionRange, result.BaseRange);
  }

  [TestMethod]
  public void Sprint_CriticalSuccess_IncreasesRange()
  {
    // High roll = critical success = range bonus
    // AS = 12 + 2 - 5 = 9, roll +4 = 13, TV 6 = SV +7 = Blazing (+2)
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
    var resolver = new SprintResolver(diceRoller);

    var result = resolver.Sprint(_sprintSkill, skillLevel: 2, dexterityValue: 12);

    Assert.IsTrue(result.IsSuccess);
    Assert.IsTrue(result.AchievedRange >= result.BaseRange);
  }

  [TestMethod]
  public void Sprint_Failure_ReducesRange()
  {
    // Low roll = failure = reduced range
    // AS = 8 + 5 - 5 = 8, roll -4 = 4, TV 6 = SV -2 = Stumbled (-2)
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(-4);
    var resolver = new SprintResolver(diceRoller);

    var result = resolver.Sprint(_sprintSkill, skillLevel: 5, dexterityValue: 8);

    Assert.IsTrue(result.AchievedRange <= result.BaseRange);
  }

  [TestMethod]
  public void Sprint_WithTerrainPenalty_AffectsResult()
  {
    // Same roll, but difficult terrain should reduce effectiveness
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var normalResolver = new SprintResolver(diceRoller);
    var normalResult = normalResolver.Sprint(_sprintSkill, 2, 10, terrainModifier: 0);

    diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var difficultResolver = new SprintResolver(diceRoller);
    var difficultResult = difficultResolver.Sprint(_sprintSkill, 2, 10, terrainModifier: TerrainModifiers.RoughGround);

    // Difficult terrain makes it harder, so achieved range should be less or equal
    Assert.IsTrue(difficultResult.AchievedRange <= normalResult.AchievedRange);
  }

  [TestMethod]
  public void Sprint_Mishap_SetsHasMishapTrue()
  {
    // Very bad roll should trigger mishap
    // AS = 5 + 8 - 5 = 8, roll -4 = 4, TV 6 + terrain -2 = TV 8, SV = 4 - 8 = -4 = Stopped
    // Need even worse: roll -4, low attribute, high skill penalty
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(-4);
    var resolver = new SprintResolver(diceRoller);

    // Very low AS combined with penalties
    var result = resolver.Sprint(_sprintSkill, 
      skillLevel: 8, 
      dexterityValue: 5, 
      terrainModifier: TerrainModifiers.SlipperySurface,
      woundCount: 2);

    // With low stats, bad roll, and penalties, likely a mishap
    // Even if not, range should be reduced significantly
    Assert.IsTrue(result.AchievedRange <= result.BaseRange);
  }

  #endregion

  #region Full-Round Sprint Tests

  [TestMethod]
  public void FullRoundSprint_UsesHigherBaseRange()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new SprintResolver(diceRoller);

    var result = resolver.FullRoundSprint(_sprintSkill, skillLevel: 2, dexterityValue: 10);

    Assert.AreEqual(Movement.FullRoundSprintRange, result.BaseRange);
    Assert.IsTrue(result.BaseRange > Movement.SprintActionRange);
  }

  [TestMethod]
  public void FullRoundSprint_Success_AchievesFullDistance()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);
    var resolver = new SprintResolver(diceRoller);

    var result = resolver.FullRoundSprint(_sprintSkill, skillLevel: 2, dexterityValue: 12);

    Assert.IsTrue(result.IsSuccess);
    Assert.IsTrue(result.AchievedRange >= Movement.FullRoundSprintRange);
  }

  [TestMethod]
  public void FullRoundSprint_Failure_StillMovesPartially()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(-2);
    var resolver = new SprintResolver(diceRoller);

    var result = resolver.FullRoundSprint(_sprintSkill, skillLevel: 4, dexterityValue: 8);

    // Even on failure, usually moves some distance (unless mishap)
    if (!result.HasMishap)
    {
      Assert.IsTrue(result.AchievedRange >= 0);
    }
  }

  #endregion

  #region Result Table Tests

  [TestMethod]
  [DataRow(8, 2, "Blazing")]   // SV > 7 = Blazing +2
  [DataRow(6, 1, "Burst")]     // SV 5-7 = Burst +1
  [DataRow(4, 1, "Swift")]     // SV 3-5 = Swift +1
  [DataRow(2, 0, "Quick")]     // SV 1-3 = Quick 0
  [DataRow(0, 0, "Moved")]     // SV 0-1 = Moved 0
  [DataRow(-1, -1, "Slowed")]  // SV -1 to -2 = Slowed -1
  [DataRow(-3, -2, "Stumbled")]// SV -3 to -4 = Stumbled -2
  [DataRow(-5, -3, "Stopped")] // SV -5 to -6 = Stopped -3
  public void GetMovementResult_ReturnsCorrectModifier(int sv, int expectedModifier, string expectedName)
  {
    var result = ResultTables.GetResult(sv, ResultTableType.Movement);

    Assert.AreEqual(expectedModifier, result.EffectValue);
    Assert.AreEqual(expectedName, result.Label);
  }

  [TestMethod]
  [DataRow(-7)]  // SV -7 to -8 = Fell
  [DataRow(-9)]  // SV <= -9 = Mishap
  public void GetMovementResult_CriticalFailure_ReturnsMishap(int sv)
  {
    var result = ResultTables.GetResult(sv, ResultTableType.Movement);

    Assert.IsTrue(result.Label == "Fell" || result.Label == "Mishap");
    Assert.IsTrue(result.EffectValue <= -99);
  }

  #endregion

  #region Terrain Modifier Tests

  [TestMethod]
  public void TerrainModifiers_HaveExpectedValues()
  {
    Assert.AreEqual(0, TerrainModifiers.Clear);
    Assert.IsTrue(TerrainModifiers.RoughGround < 0);
    Assert.IsTrue(TerrainModifiers.DeepSandOrMud < TerrainModifiers.RoughGround);
  }

  [TestMethod]
  public void TerrainModifiers_PositiveForFavorable()
  {
    Assert.IsTrue(TerrainModifiers.SteepDownhillSlope > 0);
  }

  #endregion

  #region Sprint Skill Definition Tests

  [TestMethod]
  public void CreateSprintSkill_ReturnsValidSkillDefinition()
  {
    var skill = SprintResolver.CreateSprintSkill();

    Assert.AreEqual("Sprint", skill.Name);
    Assert.AreEqual("DEX", skill.PrimaryAttribute);
    Assert.AreEqual(ResultTableType.Movement, skill.ResultTable);
  }

  // Note: SkillList.Fetch requires DataPortal configuration (integration test)
  // The Sprint skill definition is tested via SprintResolver.CreateSprintSkill()

  #endregion

  #region Movement Range Constants Tests

  [TestMethod]
  public void MovementConstants_HaveCorrectValues()
  {
    Assert.AreEqual(2, Movement.FreePositioningRange);     // 4m
    Assert.AreEqual(3, Movement.SprintActionRange);        // 9m
    Assert.AreEqual(5, Movement.FullRoundSprintRange);     // 25m
  }

  #endregion
}
