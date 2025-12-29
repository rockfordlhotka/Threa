using System.Collections.Generic;
using GameMechanics.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ActionResolverTests
{
    private IDiceRoller _zeroRoller = null!;

    [TestInitialize]
    public void Setup()
    {
        // Use deterministic dice roller that returns 0 for most tests
        _zeroRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    }

    private Skill CreateBasicSkill()
    {
        return new Skill
        {
            Id = "swords",
            Name = "Swords",
            Category = "Combat",
            PrimaryAttribute = "STR",
            ActionType = ActionType.Attack,
            TargetValueType = TargetValueType.Fixed,
            DefaultTV = 8,
            ResultTable = ResultTableType.CombatDamage,
            AppliesPhysicalityBonus = true,
            RequiresTarget = true
        };
    }

    private ActionRequest CreateBasicRequest()
    {
        return new ActionRequest
        {
            Skill = CreateBasicSkill(),
            SkillLevel = 4,
            AttributeName = "STR",
            AttributeValue = 12
        };
    }

    [TestMethod]
    public void Resolve_WithStandardAction_CalculatesCorrectly()
    {
        var request = CreateBasicRequest();
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 12 + 4 - 5 = 11
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(11, result.AbilityScore.FinalAS);
        Assert.AreEqual(8, result.TargetValue.FinalTV);
        Assert.AreEqual(1, result.Cost.AP);
        Assert.AreEqual(1, result.Cost.FAT);
    }

    [TestMethod]
    public void Resolve_WithMultipleActionPenalty_AppliesNegative1()
    {
        var request = CreateBasicRequest();
        request.IsMultipleAction = true;
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, FinalAS = 11 - 1 = 10 (multiple action penalty)
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(10, result.AbilityScore.FinalAS);
    }

    [TestMethod]
    public void Resolve_WithWounds_AppliesPenalty()
    {
        var request = CreateBasicRequest();
        request.WoundCount = 2;
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, FinalAS = 11 - 4 = 7 (2 wounds = -4)
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(7, result.AbilityScore.FinalAS);
    }

    [TestMethod]
    public void Resolve_WithAPBoost_AddsToAS()
    {
        var request = CreateBasicRequest();
        request.BoostAP = 2;
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, FinalAS = 11 + 2 = 13 (2 AP boost)
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(13, result.AbilityScore.FinalAS);
        Assert.AreEqual(2, result.Cost.BoostAP);
        Assert.AreEqual(3, result.Cost.TotalAP); // 1 base + 2 boost
    }

    [TestMethod]
    public void Resolve_WithFATBoost_AddsToAS()
    {
        var request = CreateBasicRequest();
        request.BoostFAT = 3;
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, FinalAS = 11 + 3 = 14 (3 FAT boost)
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(14, result.AbilityScore.FinalAS);
        Assert.AreEqual(3, result.Cost.BoostFAT);
        Assert.AreEqual(4, result.Cost.TotalFAT); // 1 base + 3 boost
    }

    [TestMethod]
    public void Resolve_WithAim_Adds2ToAS()
    {
        var request = CreateBasicRequest();
        request.HasAimed = true;
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, FinalAS = 11 + 2 = 13 (aim bonus)
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(13, result.AbilityScore.FinalAS);
    }

    [TestMethod]
    public void Resolve_WithCombinedModifiers_AppliesAll()
    {
        var request = CreateBasicRequest();
        request.IsMultipleAction = true;  // -1
        request.WoundCount = 1;           // -2
        request.BoostAP = 2;              // +2
        request.BoostFAT = 1;             // +1
        request.HasAimed = true;          // +2
        request.AdditionalModifiers = new List<AsModifier>
        {
            new AsModifier(ModifierSource.Equipment, "Magic Sword", 1)
        };
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        // BaseAS = 11, Final = 11 - 1 - 2 + 2 + 1 + 2 + 1 = 14
        Assert.AreEqual(11, result.AbilityScore.BaseAS);
        Assert.AreEqual(14, result.AbilityScore.FinalAS);
    }

    [TestMethod]
    public void Resolve_WithFreeAction_HasZeroCost()
    {
        var skill = CreateBasicSkill();
        skill.IsFreeAction = true;
        var request = new ActionRequest
        {
            Skill = skill,
            SkillLevel = 4,
            AttributeName = "STR",
            AttributeValue = 12
        };
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        Assert.AreEqual(0, result.Cost.AP);
        Assert.AreEqual(0, result.Cost.FAT);
        Assert.IsTrue(result.Cost.IsFreeAction);
    }

    [TestMethod]
    public void Resolve_WithSpellAction_IncludesMana()
    {
        var skill = CreateBasicSkill();
        skill.ActionType = ActionType.Spell;
        skill.ManaCost = 5;
        var request = new ActionRequest
        {
            Skill = skill,
            SkillLevel = 4,
            AttributeName = "INT",
            AttributeValue = 12
        };
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        Assert.AreEqual(5, result.Cost.Mana);
        Assert.AreEqual(ActionType.Spell, result.ActionType);
    }

    [TestMethod]
    public void CanAfford_WithSufficientResources_ReturnsTrue()
    {
        var request = CreateBasicRequest();
        request.BoostAP = 1;
        request.BoostFAT = 1;
        var resolver = new ActionResolver();
        
        // Need 2 AP (1 base + 1 boost), 2 FAT (1 base + 1 boost)
        Assert.IsTrue(resolver.CanAfford(request, 2, 2));
        Assert.IsTrue(resolver.CanAfford(request, 5, 5));
    }

    [TestMethod]
    public void CanAfford_WithInsufficientAP_ReturnsFalse()
    {
        var request = CreateBasicRequest();
        request.BoostAP = 2;
        var resolver = new ActionResolver();
        
        // Need 3 AP, only have 2
        Assert.IsFalse(resolver.CanAfford(request, 2, 5));
    }

    [TestMethod]
    public void CanAfford_WithInsufficientFAT_ReturnsFalse()
    {
        var request = CreateBasicRequest();
        request.BoostFAT = 3;
        var resolver = new ActionResolver();
        
        // Need 4 FAT, only have 3
        Assert.IsFalse(resolver.CanAfford(request, 5, 3));
    }

    [TestMethod]
    public void CalculateAbilityScore_ReturnsPreview()
    {
        var request = CreateBasicRequest();
        request.WoundCount = 1;   // -2
        request.BoostAP = 2;      // +2
        var resolver = new ActionResolver();
        
        var abilityScore = resolver.CalculateAbilityScore(request);
        
        Assert.AreEqual(11, abilityScore.BaseAS);
        Assert.AreEqual(11, abilityScore.FinalAS); // 11 - 2 + 2 = 11
    }

    [TestMethod]
    public void Resolve_WithProvidedTargetValue_UsesIt()
    {
        var request = CreateBasicRequest();
        request.TargetValue = TargetValue.Opposed(10, 2, "Goblin Dodge");
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        Assert.AreEqual(TargetValueType.Opposed, result.TargetValue.Type);
        Assert.AreEqual(10, result.TargetValue.BaseTV);
        Assert.AreEqual(2, result.TargetValue.DiceRoll);
        Assert.AreEqual(12, result.TargetValue.FinalTV); // 10 + 2 = 12
    }

    [TestMethod]
    public void Resolve_WithPassiveTV_ReducesByOne()
    {
        var request = CreateBasicRequest();
        request.TargetValue = TargetValue.Passive(10, "Surprised Guard");
        var resolver = new ActionResolver(_zeroRoller);
        
        var result = resolver.Resolve(request);
        
        Assert.AreEqual(TargetValueType.Passive, result.TargetValue.Type);
        Assert.AreEqual(9, result.TargetValue.FinalTV); // 10 - 1 = 9
    }
}
