using GameMechanics.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class MedicalResolverTests
{
    [TestMethod]
    public void ResolveCheck_WithZeroRoll_AtTV8_CalculatesSVCorrectly()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        // AS = 11 (attr 12 + skill bonus 4 - 5, or equivalent pre-computed)
        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 11,
            AttributeName = "WIL",
            ConcentrationRounds = 2
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // Total Roll = 11 (AS) + 0 (dice) = 11
        // SV = 11 - 8 (TV) = 3
        Assert.AreEqual(11, result.AbilityScore);
        Assert.AreEqual(0, result.DiceRoll);
        Assert.AreEqual(11, result.TotalRoll);
        Assert.AreEqual(3, result.SuccessValue);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void ResolveCheck_WithPositiveRoll_IncreasesSuccess()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 7,   // e.g., attr 10 + skill 2 - 5
            AttributeName = "WIL",
            ConcentrationRounds = 2
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // Total = 7 + 4 = 11
        // SV = 11 - 8 = 3
        Assert.AreEqual(7, result.AbilityScore);
        Assert.AreEqual(4, result.DiceRoll);
        Assert.AreEqual(11, result.TotalRoll);
        Assert.AreEqual(3, result.SuccessValue);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void ResolveCheck_WithNegativeRoll_CanFail()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(-4);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 7,
            AttributeName = "WIL",
            ConcentrationRounds = 2
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // Total = 7 + (-4) = 3
        // SV = 3 - 8 = -5
        Assert.AreEqual(7, result.AbilityScore);
        Assert.AreEqual(-4, result.DiceRoll);
        Assert.AreEqual(3, result.TotalRoll);
        Assert.AreEqual(-5, result.SuccessValue);
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ResolveCheck_WithWounds_PenaltyAlreadyInAbilityScore()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        // Wound penalties are pre-applied in AbilityScore via SkillEdit.AbilityScore
        // Base AS would be 11 (12 + 4 - 5), wounds reduce it to 7 (2 wounds = -4)
        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 7,   // pre-computed with wound penalties applied
            AttributeName = "WIL",
            ConcentrationRounds = 2
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // SV = 7 - 8 = -1
        Assert.AreEqual(7, result.AbilityScore);
        Assert.AreEqual(-1, result.SuccessValue);
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ResolveCheck_WithMultipleAction_AppliesPenalty()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 11,
            AttributeName = "WIL",
            ConcentrationRounds = 2,
            IsMultipleAction = true  // -1 penalty applied by resolver
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // With multiple action: 11 - 1 = 10
        // SV = 10 - 8 = 2
        Assert.AreEqual(10, result.AbilityScore);
        Assert.AreEqual(2, result.SuccessValue);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void ResolveCheck_PassesThroughConcentrationRounds(int concentrationRounds)
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillDisplayName = "Test Skill",
            AbilityScore = 7,
            AttributeName = "WIL",
            ConcentrationRounds = concentrationRounds
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert - concentration rounds are passed through from the skill definition
        Assert.AreEqual(concentrationRounds, result.ConcentrationRounds);
    }

    [TestMethod]
    [DataRow(-5, 0)]   // Failure - no healing
    [DataRow(-1, 0)]   // Failure - no healing
    [DataRow(0, 1)]    // SV 0-1: 1 healing
    [DataRow(1, 1)]
    [DataRow(2, 2)]    // SV 2-3: 2 healing
    [DataRow(3, 2)]
    [DataRow(4, 4)]    // SV 4-5: 4 healing
    [DataRow(5, 4)]
    [DataRow(6, 6)]    // SV 6-7: 6 healing
    [DataRow(7, 6)]
    [DataRow(8, 8)]    // SV 8+: 8 healing
    [DataRow(10, 8)]
    public void ResolveCheck_ReturnsCorrectHealingAmount(int targetSV, int expectedHealing)
    {
        // Arrange
        // We need AS + dice - TV = targetSV
        // With TV = 8 and AS = 10, dice needs to be targetSV - 2
        int diceRoll = targetSV + 8 - 10;
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(diceRoll);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillDisplayName = "First-Aid",
            AbilityScore = 10,
            AttributeName = "WIL",
            ConcentrationRounds = 2
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        Assert.AreEqual(targetSV, result.SuccessValue, $"SV should be {targetSV}");
        Assert.AreEqual(expectedHealing, result.HealingAmount, $"Healing for SV {targetSV} should be {expectedHealing}");
    }

    [TestMethod]
    public void MedicalTV_IsEight()
    {
        Assert.AreEqual(8, MedicalResolver.MedicalTV);
    }
}
