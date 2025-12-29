using GameMechanics.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ResultTablesTests
{
    // General Table Tests
    [TestMethod]
    public void GeneralTable_CriticalFailure()
    {
        var result = ResultTables.GetResult(-10, ResultTableType.General);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Critical Failure", result.Label);
    }

    [TestMethod]
    public void GeneralTable_SevereFailure()
    {
        var result = ResultTables.GetResult(-7, ResultTableType.General);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Severe Failure", result.Label);
    }

    [TestMethod]
    public void GeneralTable_Failure()
    {
        var result = ResultTables.GetResult(-3, ResultTableType.General);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Failure", result.Label);
    }

    [TestMethod]
    public void GeneralTable_MinorFailure()
    {
        var result = ResultTables.GetResult(-1, ResultTableType.General);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Minor Failure", result.Label);
    }

    [TestMethod]
    public void GeneralTable_MarginalSuccess()
    {
        var result = ResultTables.GetResult(0, ResultTableType.General);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Marginal Success", result.Label);
    }

    [TestMethod]
    public void GeneralTable_StandardSuccess()
    {
        var result = ResultTables.GetResult(2, ResultTableType.General);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Standard Success", result.Label);
    }

    [TestMethod]
    public void GeneralTable_GoodSuccess()
    {
        var result = ResultTables.GetResult(4, ResultTableType.General);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Good Success", result.Label);
    }

    [TestMethod]
    public void GeneralTable_ExcellentSuccess()
    {
        var result = ResultTables.GetResult(6, ResultTableType.General);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Excellent Success", result.Label);
    }

    [TestMethod]
    public void GeneralTable_OutstandingSuccess()
    {
        var result = ResultTables.GetResult(10, ResultTableType.General);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Outstanding Success", result.Label);
    }

    // Combat Damage Table Tests
    [TestMethod]
    public void CombatDamageTable_Miss()
    {
        var result = ResultTables.GetResult(-1, ResultTableType.CombatDamage);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Miss", result.Label);
    }

    [TestMethod]
    public void CombatDamageTable_Fumble()
    {
        var result = ResultTables.GetResult(-7, ResultTableType.CombatDamage);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Fumble", result.Label);
    }

    [TestMethod]
    public void CombatDamageTable_Hit()
    {
        var result = ResultTables.GetResult(3, ResultTableType.CombatDamage);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Hit", result.Label);
    }

    [TestMethod]
    public void CombatDamageTable_IncludesEffectValue()
    {
        var result = ResultTables.GetResult(5, ResultTableType.CombatDamage);
        Assert.IsTrue(result.EffectValue > 0);
    }

    // Social Table Tests
    [TestMethod]
    public void SocialTable_Unconvinced()
    {
        var result = ResultTables.GetResult(-1, ResultTableType.Social);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Unconvinced", result.Label);
    }

    [TestMethod]
    public void SocialTable_SlightlyInfluenced()
    {
        var result = ResultTables.GetResult(0, ResultTableType.Social);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Slightly Influenced", result.Label);
    }

    [TestMethod]
    public void SocialTable_Convinced()
    {
        var result = ResultTables.GetResult(2, ResultTableType.Social);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Convinced", result.Label);
    }

    // Defense Table Tests
    [TestMethod]
    public void DefenseTable_Failed()
    {
        var result = ResultTables.GetResult(-5, ResultTableType.Defense);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Defense Failed", result.Label);
    }

    [TestMethod]
    public void DefenseTable_BarelyDodged()
    {
        var result = ResultTables.GetResult(0, ResultTableType.Defense);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Barely Dodged", result.Label);
    }

    [TestMethod]
    public void DefenseTable_PerfectDefense()
    {
        var result = ResultTables.GetResult(8, ResultTableType.Defense);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Perfect Defense", result.Label);
    }

    // Crafting Table Tests
    [TestMethod]
    public void CraftingTable_Ruined()
    {
        var result = ResultTables.GetResult(-8, ResultTableType.Crafting);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Ruined", result.Label);
    }

    [TestMethod]
    public void CraftingTable_Serviceable()
    {
        var result = ResultTables.GetResult(0, ResultTableType.Crafting);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Serviceable", result.Label);
    }

    [TestMethod]
    public void CraftingTable_Masterwork()
    {
        var result = ResultTables.GetResult(10, ResultTableType.Crafting);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Masterwork", result.Label);
    }

    // Healing Table Tests
    [TestMethod]
    public void HealingTable_NoEffect()
    {
        var result = ResultTables.GetResult(-1, ResultTableType.Healing);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("No Effect", result.Label);
    }

    [TestMethod]
    public void HealingTable_HasEffectValue()
    {
        var result = ResultTables.GetResult(4, ResultTableType.Healing);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.EffectValue > 0);
    }

    // Perception Table Tests
    [TestMethod]
    public void PerceptionTable_NothingNoticed()
    {
        var result = ResultTables.GetResult(-1, ResultTableType.Perception);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Nothing Noticed", result.Label);
    }

    [TestMethod]
    public void PerceptionTable_Glimpse()
    {
        var result = ResultTables.GetResult(0, ResultTableType.Perception);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Glimpse", result.Label);
    }

    [TestMethod]
    public void PerceptionTable_PerfectAwareness()
    {
        var result = ResultTables.GetResult(10, ResultTableType.Perception);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Perfect Awareness", result.Label);
    }
}
