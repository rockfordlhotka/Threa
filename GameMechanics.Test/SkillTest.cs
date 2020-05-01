using System;
using Csla;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillTest
  {
    [TestMethod]
    public void SingleAttributeBonus()
    {
      var c = DataPortal.Create<Character>();
      c.Strength = 7;
      var cs = new CharacterSkill
      {
        PrimarySkill = "STR",
        Level = 3
      };
      var s = DataPortal.FetchChild<Skill>(cs);
      c.Skills.Add(s);
      Assert.AreEqual("STR", s.PrimaryAttribute);
      Assert.AreEqual(3, s.Level);
      Assert.AreEqual(-2, s.Bonus);
      Assert.AreEqual(5, s.AbilityScore);
    }

    [TestMethod]
    public void MultiAttributeBonus()
    {
      var c = DataPortal.Create<Character>();
      c.Strength = 7;
      var cs = new CharacterSkill
      {
        PrimarySkill = "STR/END",
        Level = 3
      };
      var s = DataPortal.FetchChild<Skill>(cs);
      c.Skills.Add(s);
      Assert.AreEqual("STR/END", s.PrimaryAttribute);
      Assert.AreEqual(3, s.Level);
      Assert.AreEqual(-2, s.Bonus);
      Assert.AreEqual(4, s.AbilityScore);
    }
  }
}
