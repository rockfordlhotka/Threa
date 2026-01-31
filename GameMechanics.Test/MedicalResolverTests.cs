using GameMechanics.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

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

        // WIL 12, Skill Level 4 => AS = 12 + 4 - 5 = 11
        var request = new MedicalRequest
        {
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 4,
            AttributeValue = 12,
            AttributeName = "WIL",
            WoundCount = 0,
            IsMultipleAction = false
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
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 2,
            AttributeValue = 10,
            AttributeName = "WIL"
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // AS = 10 + 2 - 5 = 7
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
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 2,
            AttributeValue = 10,
            AttributeName = "WIL"
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // AS = 10 + 2 - 5 = 7
        // Total = 7 + (-4) = 3
        // SV = 3 - 8 = -5
        Assert.AreEqual(7, result.AbilityScore);
        Assert.AreEqual(-4, result.DiceRoll);
        Assert.AreEqual(3, result.TotalRoll);
        Assert.AreEqual(-5, result.SuccessValue);
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void ResolveCheck_WithWounds_AppliesPenalty()
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 4,
            AttributeValue = 12,
            AttributeName = "WIL",
            WoundCount = 2  // -4 penalty
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // Base AS = 12 + 4 - 5 = 11
        // With wounds: 11 - 4 = 7
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
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 4,
            AttributeValue = 12,
            AttributeName = "WIL",
            IsMultipleAction = true  // -1 penalty
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        // Base AS = 12 + 4 - 5 = 11
        // With multiple action: 11 - 1 = 10
        // SV = 10 - 8 = 2
        Assert.AreEqual(10, result.AbilityScore);
        Assert.AreEqual(2, result.SuccessValue);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [DataRow(MedicalSkillType.FirstAid, 2)]
    [DataRow(MedicalSkillType.Nursing, 3)]
    [DataRow(MedicalSkillType.Doctor, 4)]
    public void ResolveCheck_ReturnsCorrectConcentrationRounds(MedicalSkillType skillType, int expectedRounds)
    {
        // Arrange
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillType = skillType,
            SkillLevel = 2,
            AttributeValue = 10,
            AttributeName = skillType == MedicalSkillType.FirstAid ? "WIL" : "INT"
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        Assert.AreEqual(expectedRounds, result.ConcentrationRounds);
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
        int diceRoll = targetSV + 8 - 10;  // AS of 10 (attr 10 + skill 5 - 5)
        var roller = DeterministicDiceRoller.WithFixed4dFPlus(diceRoll);
        var resolver = new MedicalResolver(roller);

        var request = new MedicalRequest
        {
            SkillType = MedicalSkillType.FirstAid,
            SkillLevel = 5,
            AttributeValue = 10,
            AttributeName = "WIL"
        };

        // Act
        var result = resolver.ResolveCheck(request);

        // Assert
        Assert.AreEqual(targetSV, result.SuccessValue, $"SV should be {targetSV}");
        Assert.AreEqual(expectedHealing, result.HealingAmount, $"Healing for SV {targetSV} should be {expectedHealing}");
    }

    [TestMethod]
    public void GetConcentrationRounds_ReturnsCorrectValues()
    {
        Assert.AreEqual(2, MedicalResolver.GetConcentrationRounds(MedicalSkillType.FirstAid));
        Assert.AreEqual(3, MedicalResolver.GetConcentrationRounds(MedicalSkillType.Nursing));
        Assert.AreEqual(4, MedicalResolver.GetConcentrationRounds(MedicalSkillType.Doctor));
    }

    [TestMethod]
    public void GetAttributeName_ReturnsCorrectAttributes()
    {
        Assert.AreEqual("WIL", MedicalResolver.GetAttributeName(MedicalSkillType.FirstAid));
        Assert.AreEqual("INT", MedicalResolver.GetAttributeName(MedicalSkillType.Nursing));
        Assert.AreEqual("INT", MedicalResolver.GetAttributeName(MedicalSkillType.Doctor));
    }

    [TestMethod]
    public void GetSkillDisplayName_ReturnsCorrectNames()
    {
        Assert.AreEqual("First-Aid", MedicalResolver.GetSkillDisplayName(MedicalSkillType.FirstAid));
        Assert.AreEqual("Nursing", MedicalResolver.GetSkillDisplayName(MedicalSkillType.Nursing));
        Assert.AreEqual("Doctor", MedicalResolver.GetSkillDisplayName(MedicalSkillType.Doctor));
    }

    [TestMethod]
    public void MedicalTV_IsEight()
    {
        Assert.AreEqual(8, MedicalResolver.MedicalTV);
    }
}
