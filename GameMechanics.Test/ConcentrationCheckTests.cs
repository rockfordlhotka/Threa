using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ConcentrationCheckTests
{
    private ServiceProvider InitServices()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddCsla();
        services.AddMockDb();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a character with a Focus skill at the specified level.
    /// Focus is a standard skill so it's already added during character creation.
    /// </summary>
    private CharacterEdit CreateCharacterWithFocusSkill(ServiceProvider provider, int focusLevel = 3)
    {
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        // Set WIL attribute to 10 for predictable AS calculations
        var wil = c.AttributeList.FirstOrDefault(a => a.Name == "WIL");
        if (wil != null) wil.Value = 10;

        // Focus is a standard skill, already added during character creation
        // Find it and set the level
        var focusSkill = c.Skills.FirstOrDefault(s =>
            s.Name.Equals("Focus", StringComparison.OrdinalIgnoreCase));

        if (focusSkill != null)
        {
            focusSkill.Level = focusLevel;
        }

        return c;
    }

    /// <summary>
    /// Adds a concentration effect to the character.
    /// </summary>
    private void AddConcentrationEffect(ServiceProvider provider, CharacterEdit c, string concentrationType = "MagazineReload")
    {
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

        string stateJson = concentrationType switch
        {
            "SustainedSpell" => ConcentrationBehavior.CreateSustainedConcentrationState("Test Spell"),
            _ => ConcentrationBehavior.CreateMagazineReloadState(Guid.NewGuid(), Guid.NewGuid(), 30, 3)
        };

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            $"Test {concentrationType}",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);
    }

    #region GetConcentrationEffect Tests

    [TestMethod]
    public void GetConcentrationEffect_WhenConcentrating_ReturnsEffect()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);
        AddConcentrationEffect(provider, c);

        // Act
        var result = c.GetConcentrationEffect();

        // Assert
        Assert.IsNotNull(result, "Should return the concentration effect");
        Assert.AreEqual(EffectType.Concentration, result.EffectType);
    }

    [TestMethod]
    public void GetConcentrationEffect_WhenNotConcentrating_ReturnsNull()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        // Act
        var result = c.GetConcentrationEffect();

        // Assert
        Assert.IsNull(result, "Should return null when not concentrating");
    }

    #endregion

    #region GetConcentrationType Tests

    [TestMethod]
    public void GetConcentrationType_WhenConcentrating_ReturnsType()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);
        AddConcentrationEffect(provider, c, "SustainedSpell");

        // Act
        var result = c.GetConcentrationType();

        // Assert
        Assert.AreEqual("SustainedSpell", result);
    }

    [TestMethod]
    public void GetConcentrationType_WhenNotConcentrating_ReturnsNull()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        // Act
        var result = c.GetConcentrationType();

        // Assert
        Assert.IsNull(result, "Should return null when not concentrating");
    }

    #endregion

    #region CheckConcentration - Not Concentrating

    [TestMethod]
    public void CheckConcentration_NotConcentrating_ReturnsSuccess()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

        // Act
        var result = c.CheckConcentration(10, 0, diceRoller);

        // Assert
        Assert.IsTrue(result.Success, "Should return success when not concentrating");
        Assert.AreEqual("Not concentrating", result.Reason);
    }

    #endregion

    #region CheckConcentration - No Focus Skill

    [TestMethod]
    public void CheckConcentration_NoFocusSkill_AutomaticFailure()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        // Remove the Focus skill (it's a standard skill added during creation)
        var focusSkill = c.Skills.FirstOrDefault(s =>
            s.Name.Equals("Focus", StringComparison.OrdinalIgnoreCase));
        if (focusSkill != null)
        {
            c.Skills.Remove(focusSkill);
        }

        AddConcentrationEffect(provider, c);
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);

        // Act
        var result = c.CheckConcentration(5, 0, diceRoller);

        // Assert
        Assert.IsFalse(result.Success, "Should fail when no Focus skill");
        Assert.AreEqual("No Focus skill", result.Reason);
        Assert.IsNull(c.GetConcentrationEffect(), "Concentration should be broken");
    }

    #endregion

    #region CheckConcentration - Successful Check

    [TestMethod]
    public void CheckConcentration_SuccessfulCheck_MaintainsConcentration()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        // Focus skill level 3 -> Bonus = -2 (skill bonus table)
        // WIL = 10
        // AS = 10 + (-2) = 8
        // With roll of +2: Result = 8 + 2 = 10
        // TV = 10, Result >= TV -> Success
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);

        // Act
        var result = c.CheckConcentration(10, 0, diceRoller);

        // Assert
        Assert.IsTrue(result.Success, "Should succeed when result >= TV");
        Assert.AreEqual(2, result.Roll);
        Assert.AreEqual(10, result.TV);
        Assert.IsNotNull(c.GetConcentrationEffect(), "Concentration should be maintained");
    }

    #endregion

    #region CheckConcentration - Failed Check

    [TestMethod]
    public void CheckConcentration_FailedCheck_BreaksConcentration()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        // With roll of -2: Result = 8 + (-2) = 6
        // TV = 10, Result < TV -> Failure
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(-2);

        // Act
        var result = c.CheckConcentration(10, 0, diceRoller);

        // Assert
        Assert.IsFalse(result.Success, "Should fail when result < TV");
        Assert.AreEqual(-2, result.Roll);
        Assert.AreEqual("Check failed", result.Reason);
        Assert.IsNull(c.GetConcentrationEffect(), "Concentration should be broken");
    }

    #endregion

    #region CheckConcentration - Damage Penalty

    [TestMethod]
    public void CheckConcentration_DamagePenalty_Applied()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        // Base AS = 8
        // 4 damage -> penalty = -2
        // Adjusted AS = 8 - 2 = 6
        // Roll = 0: Result = 6
        // TV = 7 -> Fail
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

        // Act
        var result = c.CheckConcentration(7, 4, diceRoller);

        // Assert
        Assert.AreEqual(-2, result.DamagePenalty, "Damage penalty should be -2 for 4 damage");
        Assert.AreEqual(6, result.AS, "AS should include damage penalty");
        Assert.IsFalse(result.Success, "Should fail due to damage penalty");
    }

    [TestMethod]
    public void CheckConcentration_ZeroDamage_NoPenalty()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

        // Act
        var result = c.CheckConcentration(5, 0, diceRoller);

        // Assert
        Assert.AreEqual(0, result.DamagePenalty, "Damage penalty should be 0 for 0 damage");
    }

    [TestMethod]
    public void CheckConcentration_OddDamage_PenaltyRoundsDown()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

        // Act - 3 damage should give -1 penalty (rounds down from -1.5)
        var result = c.CheckConcentration(5, 3, diceRoller);

        // Assert
        Assert.AreEqual(-1, result.DamagePenalty, "Damage penalty should be -1 for 3 damage (rounds down)");
    }

    [TestMethod]
    public void CheckConcentration_OneDamage_NoPenalty()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

        // Act - 1 damage should give 0 penalty (rounds down from -0.5)
        var result = c.CheckConcentration(5, 1, diceRoller);

        // Assert
        Assert.AreEqual(0, result.DamagePenalty, "Damage penalty should be 0 for 1 damage (rounds down)");
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void CheckConcentration_ExactlyMeetsTV_Succeeds()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        // AS = 8, Roll = 2, Result = 10, TV = 10 -> Success (exactly meets)
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);

        // Act
        var result = c.CheckConcentration(10, 0, diceRoller);

        // Assert
        Assert.IsTrue(result.Success, "Should succeed when result exactly equals TV");
        Assert.AreEqual(10, result.Result);
        Assert.AreEqual(10, result.TV);
    }

    [TestMethod]
    public void CheckConcentration_HighDamage_HighPenalty()
    {
        // Arrange
        var provider = InitServices();
        var c = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, c);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);

        // Act - 10 damage should give -5 penalty
        var result = c.CheckConcentration(5, 10, diceRoller);

        // Assert
        Assert.AreEqual(-5, result.DamagePenalty, "Damage penalty should be -5 for 10 damage");
    }

    #endregion
}
