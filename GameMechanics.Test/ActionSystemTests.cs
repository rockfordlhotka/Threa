using GameMechanics.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ActionSystemTests
{
    [TestMethod]
    public void ActionCost_Standard_Returns1AP1FAT()
    {
        var cost = ActionCost.Standard();
        
        Assert.AreEqual(1, cost.AP);
        Assert.AreEqual(1, cost.FAT);
        Assert.AreEqual(0, cost.Mana);
        Assert.IsFalse(cost.IsFreeAction);
    }

    [TestMethod]
    public void ActionCost_FatigueFree_Returns2AP0FAT()
    {
        var cost = ActionCost.FatigueFree();
        
        Assert.AreEqual(2, cost.AP);
        Assert.AreEqual(0, cost.FAT);
        Assert.IsFalse(cost.IsFreeAction);
    }

    [TestMethod]
    public void ActionCost_Free_ReturnsZeroCost()
    {
        var cost = ActionCost.Free();
        
        Assert.AreEqual(0, cost.AP);
        Assert.AreEqual(0, cost.FAT);
        Assert.IsTrue(cost.IsFreeAction);
    }

    [TestMethod]
    public void ActionCost_WithBoosts_AddsToBothCostAndBonus()
    {
        var cost = ActionCost.Standard()
            .WithAPBoost(1)
            .WithFATBoost(2);
        
        Assert.AreEqual(1, cost.AP);
        Assert.AreEqual(1, cost.FAT);
        Assert.AreEqual(1, cost.BoostAP);
        Assert.AreEqual(2, cost.BoostFAT);
        Assert.AreEqual(2, cost.TotalAP);  // 1 base + 1 boost
        Assert.AreEqual(3, cost.TotalFAT); // 1 base + 2 boost
        Assert.AreEqual(3, cost.BoostBonus); // 1 AP + 2 FAT boost = +3 AS
    }

    [TestMethod]
    public void AbilityScore_CalculatesBaseAS_Correctly()
    {
        var abilityScore = new AbilityScore
        {
            AttributeValue = 12,
            SkillLevel = 4,
            SkillName = "Swords",
            AttributeName = "STR"
        };
        
        // AS = Attribute + Skill - 5 = 12 + 4 - 5 = 11
        Assert.AreEqual(11, abilityScore.BaseAS);
        Assert.AreEqual(11, abilityScore.FinalAS); // No modifiers yet
    }

    [TestMethod]
    public void AbilityScore_AppliesMultipleActionPenalty()
    {
        var abilityScore = new AbilityScore
        {
            AttributeValue = 12,
            SkillLevel = 4
        };
        
        abilityScore.ApplyMultipleActionPenalty();
        
        Assert.AreEqual(11, abilityScore.BaseAS);
        Assert.AreEqual(10, abilityScore.FinalAS); // 11 - 1 = 10
    }

    [TestMethod]
    public void AbilityScore_AppliesWoundPenalties()
    {
        var abilityScore = new AbilityScore
        {
            AttributeValue = 12,
            SkillLevel = 4
        };
        
        abilityScore.ApplyWoundPenalties(2); // 2 wounds = -4 AS
        
        Assert.AreEqual(11, abilityScore.BaseAS);
        Assert.AreEqual(7, abilityScore.FinalAS); // 11 - 4 = 7
    }

    [TestMethod]
    public void AbilityScore_AppliesBoostBonus()
    {
        var abilityScore = new AbilityScore
        {
            AttributeValue = 10,
            SkillLevel = 3
        };
        
        abilityScore.ApplyBoostBonus(3); // Spend 3 resources for +3 AS
        
        Assert.AreEqual(8, abilityScore.BaseAS); // 10 + 3 - 5 = 8
        Assert.AreEqual(11, abilityScore.FinalAS); // 8 + 3 = 11
    }

    [TestMethod]
    public void AbilityScore_AppliesAimBonus()
    {
        var abilityScore = new AbilityScore
        {
            AttributeValue = 10,
            SkillLevel = 3
        };
        
        abilityScore.ApplyAimBonus();
        
        Assert.AreEqual(8, abilityScore.BaseAS);
        Assert.AreEqual(10, abilityScore.FinalAS); // 8 + 2 = 10
    }

    [TestMethod]
    public void ModifierStack_AggregatesCorrectly()
    {
        var stack = new ModifierStack();
        stack.Add(ModifierSource.Boost, "Boost", 2);
        stack.Add(ModifierSource.Wound, "Wound", -4);
        stack.Add(ModifierSource.Equipment, "Magic Sword", 1);
        
        Assert.AreEqual(-1, stack.Total);
        Assert.AreEqual(3, stack.TotalBonuses);
        Assert.AreEqual(-4, stack.TotalPenalties);
    }

    [TestMethod]
    public void TargetValue_Fixed_CreatesCorrectTV()
    {
        var tv = TargetValue.Fixed(8, "Moderate Difficulty");
        
        Assert.AreEqual(TargetValueType.Fixed, tv.Type);
        Assert.AreEqual(8, tv.BaseTV);
        Assert.AreEqual(0, tv.DiceRoll);
        Assert.AreEqual(8, tv.FinalTV);
    }

    [TestMethod]
    public void TargetValue_Opposed_IncludesDiceRoll()
    {
        var tv = TargetValue.Opposed(10, 2, "Goblin Dodge");
        
        Assert.AreEqual(TargetValueType.Opposed, tv.Type);
        Assert.AreEqual(10, tv.BaseTV);
        Assert.AreEqual(2, tv.DiceRoll);
        Assert.AreEqual(12, tv.FinalTV); // 10 + 2 = 12
    }

    [TestMethod]
    public void TargetValue_Passive_SubtractsOne()
    {
        var tv = TargetValue.Passive(10, "Surprised Guard");
        
        Assert.AreEqual(TargetValueType.Passive, tv.Type);
        Assert.AreEqual(9, tv.BaseTV); // 10 - 1 = 9
        Assert.AreEqual(9, tv.FinalTV);
    }

    [TestMethod]
    public void TargetValue_FromDifficulty_UsesEnumValue()
    {
        var tv = TargetValue.FromDifficulty(DifficultyLevel.Hard);
        
        Assert.AreEqual(12, tv.FinalTV);
    }

    [TestMethod]
    public void ActionResult_CalculatesSV_Correctly()
    {
        var result = new ActionResult
        {
            SkillName = "Swords",
            ActionType = ActionType.Attack,
            DiceRoll = 2
        };
        result.AbilityScore.AttributeValue = 12;
        result.AbilityScore.SkillLevel = 4;
        result.TargetValue = TargetValue.Fixed(8, "Target");
        
        // RollResult = AS + DiceRoll = 11 + 2 = 13
        // SV = 13 - 8 = 5
        Assert.AreEqual(13, result.RollResult);
        Assert.AreEqual(5, result.SuccessValue);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Good Success", result.ResultQuality);
    }

    [TestMethod]
    public void ActionResult_DetectsFailure()
    {
        var result = new ActionResult
        {
            SkillName = "Dodge",
            DiceRoll = -3
        };
        result.AbilityScore.AttributeValue = 10;
        result.AbilityScore.SkillLevel = 2;
        result.TargetValue = TargetValue.Fixed(12, "Attack");
        
        // RollResult = AS + DiceRoll = 7 + (-3) = 4
        // SV = 4 - 12 = -8
        Assert.AreEqual(4, result.RollResult);
        Assert.AreEqual(-8, result.SuccessValue);
        Assert.IsFalse(result.IsSuccess);
    }
}
