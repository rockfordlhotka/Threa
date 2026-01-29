using Csla;
using Csla.Configuration;
using GameMechanics.Combat;
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
public class DefenseConcentrationTests
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
    /// </summary>
    private CharacterEdit CreateCharacterWithFocusSkill(ServiceProvider provider, int focusLevel = 3)
    {
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        // Set WIL attribute to 10 for predictable AS calculations
        var wil = c.AttributeList.FirstOrDefault(a => a.Name == "WIL");
        if (wil != null) wil.Value = 10;

        // Focus is a standard skill, find it and set the level
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

    #region Passive Defense Concentration Check Tests

    [TestMethod]
    public void PassiveDefense_WhenConcentrating_TriggersConcentrationCheck()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 12,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsNotNull(result.ConcentrationCheck, "Should have concentration check result");
        Assert.AreEqual(12, result.ConcentrationCheck.TV, "TV should be attacker's AV");
    }

    [TestMethod]
    public void PassiveDefense_SuccessfulCheck_MaintainsConcentration()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        // Focus AS = WIL(10) + skill bonus(-2) = 8
        // Roll +4 = Result 12 vs TV 10 -> Success
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 10,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsTrue(result.ConcentrationCheck!.Success, "Check should succeed");
        Assert.IsFalse(result.ConcentrationBroken, "Concentration should not be broken");
        Assert.IsNotNull(defender.GetConcentrationEffect(), "Should still have concentration effect");
    }

    [TestMethod]
    public void PassiveDefense_FailedCheck_BreaksConcentration()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        // Focus AS = 8, Roll -2 = Result 6 vs TV 10 -> Failure
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(-2);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 10,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsFalse(result.ConcentrationCheck!.Success, "Check should fail");
        Assert.IsTrue(result.ConcentrationBroken, "Concentration should be broken");
        Assert.IsNull(defender.GetConcentrationEffect(), "Concentration effect should be removed");
    }

    [TestMethod]
    public void PassiveDefense_DamagePenalty_AppliedToCheck()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        // Focus AS = 8
        // 6 damage -> penalty = -3
        // Adjusted AS = 8 - 3 = 5
        // Roll 0 = Result 5 vs TV 6 -> Failure
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 6,
            DamageDealt = 6
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.AreEqual(-3, result.ConcentrationCheck!.DamagePenalty, "Damage penalty should be -3 for 6 damage");
        Assert.AreEqual(5, result.ConcentrationCheck.AS, "AS should include damage penalty");
        Assert.IsFalse(result.ConcentrationCheck.Success, "Check should fail due to damage penalty");
    }

    [TestMethod]
    public void PassiveDefense_NoDamage_NoPenalty()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 5,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.AreEqual(0, result.ConcentrationCheck!.DamagePenalty, "No damage penalty for 0 damage");
        Assert.AreEqual(8, result.ConcentrationCheck.AS, "AS should be base Focus AS without penalty");
    }

    #endregion

    #region Active Defense Concentration Tests

    [TestMethod]
    public void ActiveDefense_WhenConcentrating_BreaksBeforeRoll()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Dodge,
            DodgeAS = 10,
            Defender = defender
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsTrue(result.ConcentrationBroken, "Concentration should be broken");
        Assert.IsNull(result.ConcentrationCheck, "No concentration check for active defense");
        Assert.IsNull(defender.GetConcentrationEffect(), "Concentration effect should be removed");
    }

    [TestMethod]
    public void ActiveParry_WhenConcentrating_BreaksBeforeRoll()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        AddConcentrationEffect(provider, defender);

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Parry,
            ParryAS = 12,
            Defender = defender,
            IsRangedAttack = false
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsTrue(result.ConcentrationBroken, "Concentration should be broken");
        Assert.IsNull(result.ConcentrationCheck, "No concentration check for active defense");
        Assert.IsNull(defender.GetConcentrationEffect(), "Concentration effect should be removed");
    }

    #endregion

    #region Health Depletion Auto-Break Tests

    [TestMethod]
    public void PassiveDefense_FatigueExhausted_AutoBreaks()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 5);
        AddConcentrationEffect(provider, defender);

        // Deplete fatigue pool
        defender.Fatigue.PendingDamage = defender.Fatigue.Value;

        // Roll high enough to pass the check normally
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 8,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsTrue(result.ConcentrationCheck!.Success, "Concentration check should pass");
        Assert.IsTrue(result.ConcentrationBroken, "Concentration should be broken due to exhaustion");
        Assert.IsNull(defender.GetConcentrationEffect(), "Concentration effect should be removed");
    }

    [TestMethod]
    public void PassiveDefense_VitalityExhausted_AutoBreaks()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 5);
        AddConcentrationEffect(provider, defender);

        // Deplete vitality pool
        defender.Vitality.PendingDamage = defender.Vitality.Value;

        // Roll high enough to pass the check normally
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 8,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsTrue(result.ConcentrationCheck!.Success, "Concentration check should pass");
        Assert.IsTrue(result.ConcentrationBroken, "Concentration should be broken due to exhaustion");
        Assert.IsNull(defender.GetConcentrationEffect(), "Concentration effect should be removed");
    }

    #endregion

    #region Not Concentrating Tests

    [TestMethod]
    public void PassiveDefense_NotConcentrating_NoCheck()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        // NOT adding concentration effect

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Passive,
            DodgeAS = 10,
            Defender = defender,
            AttackerAV = 12,
            DamageDealt = 0
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsNull(result.ConcentrationCheck, "No concentration check when not concentrating");
        Assert.IsFalse(result.ConcentrationBroken, "Concentration not broken when not concentrating");
    }

    [TestMethod]
    public void ActiveDefense_NotConcentrating_NoBreak()
    {
        // Arrange
        var provider = InitServices();
        var defender = CreateCharacterWithFocusSkill(provider, focusLevel: 3);
        // NOT adding concentration effect

        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);
        var resolver = new DefenseResolver(diceRoller);

        var request = new DefenseRequest
        {
            DefenseType = DefenseType.Dodge,
            DodgeAS = 10,
            Defender = defender
        };

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsNull(result.ConcentrationCheck, "No concentration check");
        Assert.IsFalse(result.ConcentrationBroken, "No concentration to break");
    }

    #endregion

    #region No Defender Tests

    [TestMethod]
    public void PassiveDefense_NoDefender_NoCheck()
    {
        // Arrange - no defender provided (legacy usage)
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
        var resolver = new DefenseResolver(diceRoller);

        var request = DefenseRequest.Passive(dodgeAS: 10);

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsNull(result.ConcentrationCheck, "No concentration check without defender");
        Assert.IsFalse(result.ConcentrationBroken, "No break without defender");
        Assert.AreEqual(9, result.TV, "TV should still be calculated correctly");
    }

    [TestMethod]
    public void ActiveDefense_NoDefender_NoBreak()
    {
        // Arrange - no defender provided (legacy usage)
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(3);
        var resolver = new DefenseResolver(diceRoller);

        var request = DefenseRequest.Dodge(dodgeAS: 10);

        // Act
        var result = resolver.Resolve(request);

        // Assert
        Assert.IsNull(result.ConcentrationCheck, "No concentration check without defender");
        Assert.IsFalse(result.ConcentrationBroken, "No break without defender");
        Assert.AreEqual(13, result.TV, "TV should still be calculated correctly");
    }

    #endregion
}
