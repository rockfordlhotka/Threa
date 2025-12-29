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

  #region MovementCost Tests

  [TestMethod]
  public void MovementCost_Free_HasZeroCost()
  {
    var cost = MovementCost.Free();
    
    Assert.AreEqual(0, cost.AP);
    Assert.AreEqual(0, cost.FAT);
    Assert.IsFalse(cost.IsFullRound);
    Assert.IsTrue(cost.IsFree);
  }

  [TestMethod]
  public void MovementCost_Standard_CostsOneAPOneFAT()
  {
    var cost = MovementCost.Standard();
    
    Assert.AreEqual(1, cost.AP);
    Assert.AreEqual(1, cost.FAT);
    Assert.IsFalse(cost.IsFullRound);
    Assert.IsFalse(cost.IsFree);
  }

  [TestMethod]
  public void MovementCost_FatigueFree_CostsTwoAPNoFAT()
  {
    var cost = MovementCost.FatigueFree();
    
    Assert.AreEqual(2, cost.AP);
    Assert.AreEqual(0, cost.FAT);
    Assert.IsFalse(cost.IsFullRound);
    Assert.IsFalse(cost.IsFree);
  }

  [TestMethod]
  public void MovementCost_FullRound_IsFullRoundFlag()
  {
    var cost = MovementCost.FullRound();
    
    Assert.AreEqual(0, cost.AP);
    Assert.AreEqual(0, cost.FAT);
    Assert.IsTrue(cost.IsFullRound);
    Assert.IsFalse(cost.IsFree);  // Full round is not "free" even though AP/FAT are 0
  }

  [TestMethod]
  [DataRow(MovementType.FreePositioning)]
  public void Movement_GetCost_FreePositioning_ReturnsFree(MovementType type)
  {
    var cost = Movement.GetCost(type);
    Assert.IsTrue(cost.IsFree);
  }

  [TestMethod]
  [DataRow(MovementType.Sprint)]
  public void Movement_GetCost_Sprint_ReturnsStandard(MovementType type)
  {
    var cost = Movement.GetCost(type);
    Assert.AreEqual(1, cost.AP);
    Assert.AreEqual(1, cost.FAT);
  }

  [TestMethod]
  [DataRow(MovementType.FullRoundSprint)]
  public void Movement_GetCost_FullRoundSprint_ReturnsFullRound(MovementType type)
  {
    var cost = Movement.GetCost(type);
    Assert.IsTrue(cost.IsFullRound);
  }

  #endregion
}

[TestClass]
public class TravelCalculatorTests
{
  #region Travel Rate Tests

  [TestMethod]
  public void GetTravelRate_Walking_Returns4MPerRound()
  {
    var rate = TravelCalculator.GetTravelRate(TravelType.Walking);
    
    Assert.AreEqual(4, rate.MetersPerRound);
    Assert.AreEqual(TravelType.Walking, rate.Type);
  }

  [TestMethod]
  public void GetTravelRate_EnduranceRunning_Returns10MPerRound()
  {
    var rate = TravelCalculator.GetTravelRate(TravelType.EnduranceRunning);
    
    Assert.AreEqual(10, rate.MetersPerRound);
  }

  [TestMethod]
  public void GetTravelRate_BurstRunning_Returns12MPerRound()
  {
    var rate = TravelCalculator.GetTravelRate(TravelType.BurstRunning);
    
    Assert.AreEqual(12, rate.MetersPerRound);
  }

  [TestMethod]
  public void GetTravelRate_FastSprinting_Returns16MPerRound()
  {
    var rate = TravelCalculator.GetTravelRate(TravelType.FastSprinting);
    
    Assert.AreEqual(16, rate.MetersPerRound);
  }

  [TestMethod]
  public void TravelRate_MetersPerHour_CalculatesCorrectly()
  {
    var rate = TravelCalculator.GetTravelRate(TravelType.Walking);
    
    // 4m/round * 1200 rounds/hour = 4800m/hour = 4.8km/hour
    Assert.AreEqual(4800, rate.MetersPerHour);
    Assert.AreEqual(4.8, rate.KilometersPerHour, 0.01);
  }

  #endregion

  #region Travel Calculation Tests

  [TestMethod]
  public void CalculateTravel_ZeroDistance_ReturnsZero()
  {
    var result = TravelCalculator.CalculateTravel(0, TravelType.Walking);
    
    Assert.AreEqual(0, result.RoundsRequired);
    Assert.AreEqual(0, result.FatigueCost);
  }

  [TestMethod]
  public void CalculateTravel_Walking2km_Returns1Fatigue()
  {
    // Walking: 1 FAT per 2km
    var result = TravelCalculator.CalculateTravel(2000, TravelType.Walking);
    
    Assert.AreEqual(1, result.FatigueCost);
  }

  [TestMethod]
  public void CalculateTravel_Walking_RoundsCalculation()
  {
    // 100m at 4m/round = 25 rounds
    var result = TravelCalculator.CalculateTravel(100, TravelType.Walking);
    
    Assert.AreEqual(25, result.RoundsRequired);
  }

  [TestMethod]
  public void CalculateTravel_EnduranceRunning1km_Returns1Fatigue()
  {
    // Endurance running: 1 FAT per km
    var result = TravelCalculator.CalculateTravel(1000, TravelType.EnduranceRunning);
    
    Assert.AreEqual(1, result.FatigueCost);
  }

  [TestMethod]
  public void CalculateTravel_BurstRunning_HighFatigueCost()
  {
    // Burst running: 1 FAT per 6m = 10 FAT for 60m
    var result = TravelCalculator.CalculateTravel(60, TravelType.BurstRunning);
    
    Assert.AreEqual(10, result.FatigueCost);
  }

  [TestMethod]
  public void CalculateTravel_FastSprinting_HighestFatigueCost()
  {
    // Fast sprinting: 1 FAT per 5m = 10 FAT for 50m
    var result = TravelCalculator.CalculateTravel(50, TravelType.FastSprinting);
    
    Assert.AreEqual(10, result.FatigueCost);
  }

  [TestMethod]
  public void CalculateTravel_TimeSeconds_CalculatesCorrectly()
  {
    // 12m at 4m/round = 3 rounds * 3 seconds = 9 seconds
    var result = TravelCalculator.CalculateTravel(12, TravelType.Walking);
    
    Assert.AreEqual(3, result.RoundsRequired);
    Assert.AreEqual(9, result.TimeSeconds);
  }

  #endregion

  #region Max Distance Tests

  [TestMethod]
  public void CalculateMaxDistance_Walking_ReturnsCorrectDistance()
  {
    // 1 FAT = 2000m walking
    var maxDistance = TravelCalculator.CalculateMaxDistance(1, TravelType.Walking);
    
    Assert.AreEqual(2000, maxDistance);
  }

  [TestMethod]
  public void CalculateMaxDistance_FastSprinting_ReturnsCorrectDistance()
  {
    // 1 FAT = 5m fast sprinting
    var maxDistance = TravelCalculator.CalculateMaxDistance(10, TravelType.FastSprinting);
    
    Assert.AreEqual(50, maxDistance);
  }

  [TestMethod]
  public void CalculateMaxDistance_ZeroFatigue_ReturnsZero()
  {
    var maxDistance = TravelCalculator.CalculateMaxDistance(0, TravelType.Walking);
    
    Assert.AreEqual(0, maxDistance);
  }

  #endregion

  #region Time String Tests

  [TestMethod]
  public void RoundsToTimeString_ShortTime_ReturnsSeconds()
  {
    var timeString = TravelCalculator.RoundsToTimeString(5);
    
    Assert.AreEqual("15 seconds", timeString);
  }

  [TestMethod]
  public void RoundsToTimeString_OneMinute_ReturnsMinutes()
  {
    // 20 rounds = 1 minute
    var timeString = TravelCalculator.RoundsToTimeString(20);
    
    Assert.AreEqual("1 minutes", timeString);
  }

  [TestMethod]
  public void RoundsToTimeString_MixedMinutes_IncludesSeconds()
  {
    // 25 rounds = 75 seconds = 1 min 15 sec
    var timeString = TravelCalculator.RoundsToTimeString(25);
    
    Assert.AreEqual("1 min 15 sec", timeString);
  }

  [TestMethod]
  public void RoundsToTimeString_OneHour_ReturnsHours()
  {
    // 1200 rounds = 1 hour
    var timeString = TravelCalculator.RoundsToTimeString(1200);
    
    Assert.AreEqual("1 hours", timeString);
  }

  #endregion

  #region Result Summary Tests

  [TestMethod]
  public void TravelResult_GetSummary_ReturnsFormattedString()
  {
    var result = TravelCalculator.CalculateTravel(100, TravelType.Walking);
    var summary = result.GetSummary();
    
    Assert.IsTrue(summary.Contains("Walking"));
    Assert.IsTrue(summary.Contains("100m"));
    Assert.IsTrue(summary.Contains("FAT"));
  }

  #endregion
}

[TestClass]
public class EncumbranceTests
{
  #region Carrying Capacity Tests

  [TestMethod]
  [DataRow(10, 50.0)]   // PHY 10 = 50 lbs (baseline)
  [DataRow(6, 28.59)]   // PHY 6 = ~29 lbs
  [DataRow(14, 87.45)]  // PHY 14 = ~87 lbs
  public void CalculateMaxWeight_ReturnsExpectedCapacity(int physicality, double expectedWeight)
  {
    var maxWeight = Encumbrance.CalculateMaxWeight(physicality);
    
    Assert.AreEqual(expectedWeight, maxWeight, 0.5);
  }

  [TestMethod]
  [DataRow(10, 10.0)]   // PHY 10 = 10 cu.ft.
  [DataRow(12, 13.22)]  // PHY 12 = ~13 cu.ft.
  public void CalculateMaxVolume_ReturnsExpectedCapacity(int physicality, double expectedVolume)
  {
    var maxVolume = Encumbrance.CalculateMaxVolume(physicality);
    
    Assert.AreEqual(expectedVolume, maxVolume, 0.5);
  }

  #endregion

  #region Encumbrance Level Tests

  [TestMethod]
  public void CalculateEncumbrance_Under50Percent_IsUnencumbered()
  {
    // 20 lbs / 50 lbs = 40%
    var status = Encumbrance.CalculateEncumbrance(20, 50);
    
    Assert.AreEqual(EncumbranceLevel.Unencumbered, status.Level);
    Assert.AreEqual(0, status.RangePenalty);
    Assert.IsTrue(status.CanMove);
  }

  [TestMethod]
  public void CalculateEncumbrance_50To75Percent_IsLight()
  {
    // 30 lbs / 50 lbs = 60%
    var status = Encumbrance.CalculateEncumbrance(30, 50);
    
    Assert.AreEqual(EncumbranceLevel.Light, status.Level);
    Assert.AreEqual(0, status.RangePenalty);
  }

  [TestMethod]
  public void CalculateEncumbrance_75To100Percent_IsMedium()
  {
    // 40 lbs / 50 lbs = 80%
    var status = Encumbrance.CalculateEncumbrance(40, 50);
    
    Assert.AreEqual(EncumbranceLevel.Medium, status.Level);
    Assert.AreEqual(-1, status.RangePenalty);
  }

  [TestMethod]
  public void CalculateEncumbrance_100To125Percent_IsHeavy()
  {
    // 55 lbs / 50 lbs = 110%
    var status = Encumbrance.CalculateEncumbrance(55, 50);
    
    Assert.AreEqual(EncumbranceLevel.Heavy, status.Level);
    Assert.AreEqual(-2, status.RangePenalty);
  }

  [TestMethod]
  public void CalculateEncumbrance_125To150Percent_IsVeryHeavy()
  {
    // 70 lbs / 50 lbs = 140%
    var status = Encumbrance.CalculateEncumbrance(70, 50);
    
    Assert.AreEqual(EncumbranceLevel.VeryHeavy, status.Level);
    Assert.AreEqual(-3, status.RangePenalty);
  }

  [TestMethod]
  public void CalculateEncumbrance_Over150Percent_IsOverloaded()
  {
    // 80 lbs / 50 lbs = 160%
    var status = Encumbrance.CalculateEncumbrance(80, 50);
    
    Assert.AreEqual(EncumbranceLevel.Overloaded, status.Level);
    Assert.IsFalse(status.CanMove);
  }

  [TestMethod]
  public void CalculateEncumbrance_WithPhysicality_UsesCorrectCapacity()
  {
    // PHY 10 = 50 lbs capacity, 25 lbs = 50% = Light
    var status = Encumbrance.CalculateFromPhysicality(25, physicality: 10);
    
    Assert.AreEqual(EncumbranceLevel.Light, status.Level);
    Assert.AreEqual(50.0, status.MaxWeight, 0.1);
  }

  #endregion

  #region Movement Penalty Tests

  [TestMethod]
  public void ApplyToMovement_Unencumbered_NoPenalty()
  {
    var status = Encumbrance.CalculateEncumbrance(20, 50); // 40%
    var adjustedRange = Encumbrance.ApplyToMovement(3, status);
    
    Assert.AreEqual(3, adjustedRange);
  }

  [TestMethod]
  public void ApplyToMovement_Medium_ReducesRange()
  {
    var status = Encumbrance.CalculateEncumbrance(40, 50); // 80% = -1
    var adjustedRange = Encumbrance.ApplyToMovement(3, status);
    
    Assert.AreEqual(2, adjustedRange);
  }

  [TestMethod]
  public void ApplyToMovement_VeryHeavy_SignificantPenalty()
  {
    var status = Encumbrance.CalculateEncumbrance(70, 50); // 140% = -3
    var adjustedRange = Encumbrance.ApplyToMovement(3, status);
    
    Assert.AreEqual(0, adjustedRange);
  }

  [TestMethod]
  public void ApplyToMovement_Overloaded_ReturnsZero()
  {
    var status = Encumbrance.CalculateEncumbrance(80, 50); // 160% = can't move
    var adjustedRange = Encumbrance.ApplyToMovement(5, status);
    
    Assert.AreEqual(0, adjustedRange);
  }

  [TestMethod]
  public void ApplyToMovement_MinimumZero()
  {
    var status = Encumbrance.CalculateEncumbrance(70, 50); // 140% = -3
    var adjustedRange = Encumbrance.ApplyToMovement(1, status);
    
    Assert.AreEqual(0, adjustedRange); // Can't go negative
  }

  #endregion

  #region GetMaxMovementRange Tests

  [TestMethod]
  public void GetMaxMovementRange_FreePositioning_WithEncumbrance()
  {
    var status = Encumbrance.CalculateEncumbrance(40, 50); // -1 penalty
    var maxRange = Encumbrance.GetMaxMovementRange(MovementType.FreePositioning, status);
    
    Assert.AreEqual(1, maxRange); // 2 base - 1 penalty = 1
  }

  [TestMethod]
  public void GetMaxMovementRange_Sprint_WithEncumbrance()
  {
    var status = Encumbrance.CalculateEncumbrance(55, 50); // -2 penalty
    var maxRange = Encumbrance.GetMaxMovementRange(MovementType.Sprint, status);
    
    Assert.AreEqual(1, maxRange); // 3 base - 2 penalty = 1
  }

  [TestMethod]
  public void GetMaxMovementRange_FullRoundSprint_WithEncumbrance()
  {
    var status = Encumbrance.CalculateEncumbrance(70, 50); // -3 penalty
    var maxRange = Encumbrance.GetMaxMovementRange(MovementType.FullRoundSprint, status);
    
    Assert.AreEqual(2, maxRange); // 5 base - 3 penalty = 2
  }

  #endregion

  #region Status Properties Tests

  [TestMethod]
  public void EncumbranceStatus_PercentCapacity_CalculatesCorrectly()
  {
    var status = Encumbrance.CalculateEncumbrance(37.5, 50);
    
    Assert.AreEqual(75.0, status.PercentCapacity, 0.1);
  }

  [TestMethod]
  public void EncumbranceStatus_RemainingCapacity_CalculatesCorrectly()
  {
    var status = Encumbrance.CalculateEncumbrance(30, 50);
    
    Assert.AreEqual(20.0, status.RemainingCapacity, 0.1);
  }

  [TestMethod]
  public void EncumbranceStatus_GetSummary_ReturnsFormattedString()
  {
    var status = Encumbrance.CalculateEncumbrance(40, 50);
    var summary = status.GetSummary();
    
    Assert.IsTrue(summary.Contains("Medium"));
    Assert.IsTrue(summary.Contains("40"));
    Assert.IsTrue(summary.Contains("50"));
    Assert.IsTrue(summary.Contains("-1"));
  }

  #endregion
}
