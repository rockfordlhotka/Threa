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
            Assert.AreEqual(60, SkillCost.GetLevelUpCost(1, 11));
        }

        [TestMethod]
        public void GetCost1x12()
        {
            Assert.AreEqual(70, SkillCost.GetLevelUpCost(1, 12));
        }

        [TestMethod]
        public void GetCumulativeCost()
        {
            // level 0->1 cost for diff 11 is 40
            // level 1->2 cost for diff 11 is 60
            // cumulative to reach level 2 is 100
            Assert.AreEqual(100, SkillCost.GetCumulativeCost(2, 11));
        }
    }
}
