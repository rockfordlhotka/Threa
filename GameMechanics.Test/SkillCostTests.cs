using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillCostTests
  {
    [TestMethod]
    public void GetBonusValid()
    {
      Assert.AreEqual(0, SkillCost.GetBonus(5));
    }

    [TestMethod]
    public void GetBonusTooLow()
    {
      Assert.ThrowsExactly<ArgumentException>(() => SkillCost.GetBonus(-2));
    }

    [TestMethod]
    public void GetBonusTooHigh()
    {
      Assert.ThrowsExactly<ArgumentException>(() => SkillCost.GetBonus(11));
    }

    [TestMethod]
    public void GetCost1x11()
    {
      Assert.AreEqual(6, SkillCost.GetLevelUpCost(1, 11));
    }

    [TestMethod]
    public void GetCost1x12()
    {
      Assert.AreEqual(7, SkillCost.GetLevelUpCost(1, 12));
    }
  }
}
