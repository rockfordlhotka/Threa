using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class ActionPointsTests
{
    [TestMethod]
    [DataRow(0, 1, DisplayName = "Zero skill levels returns minimum 1")]
    [DataRow(-5, 1, DisplayName = "Negative skill levels returns minimum 1")]
    [DataRow(10, 1, DisplayName = "10 skill levels returns 1 (10/20 = 0, but minimum 1)")]
    [DataRow(19, 1, DisplayName = "19 skill levels returns 1 (19/20 = 0, but minimum 1)")]
    [DataRow(20, 1, DisplayName = "20 skill levels returns 1 (20/20 = 1)")]
    [DataRow(21, 1, DisplayName = "21 skill levels returns 1 (21/20 = 1)")]
    [DataRow(39, 1, DisplayName = "39 skill levels returns 1 (39/20 = 1)")]
    [DataRow(40, 2, DisplayName = "40 skill levels returns 2 (40/20 = 2)")]
    [DataRow(47, 2, DisplayName = "47 skill levels returns 2 (47/20 = 2)")]
    [DataRow(60, 3, DisplayName = "60 skill levels returns 3 (60/20 = 3)")]
    [DataRow(100, 5, DisplayName = "100 skill levels returns 5 (100/20 = 5)")]
    [DataRow(120, 6, DisplayName = "120 skill levels returns 6 (120/20 = 6)")]
    public void CalculateMax_ReturnsExpectedMaxAP(int totalSkillLevels, int expectedMaxAP)
    {
        var result = ActionPoints.CalculateMax(totalSkillLevels);
        Assert.AreEqual(expectedMaxAP, result);
    }

    [TestMethod]
    public void CalculateRecovery_ReturnsMinimum1_WhenFatigueLessThan4()
    {
        Assert.AreEqual(1, ActionPoints.CalculateRecovery(0));
        Assert.AreEqual(1, ActionPoints.CalculateRecovery(1));
        Assert.AreEqual(1, ActionPoints.CalculateRecovery(2));
        Assert.AreEqual(1, ActionPoints.CalculateRecovery(3));
    }

    [TestMethod]
    [DataRow(4, 1, DisplayName = "FAT 4 recovers 1 AP")]
    [DataRow(8, 2, DisplayName = "FAT 8 recovers 2 AP")]
    [DataRow(12, 3, DisplayName = "FAT 12 recovers 3 AP")]
    [DataRow(16, 4, DisplayName = "FAT 16 recovers 4 AP")]
    [DataRow(20, 5, DisplayName = "FAT 20 recovers 5 AP")]
    public void CalculateRecovery_ReturnsExpectedRecovery(int fatigue, int expectedRecovery)
    {
        var result = ActionPoints.CalculateRecovery(fatigue);
        Assert.AreEqual(expectedRecovery, result);
    }
}
