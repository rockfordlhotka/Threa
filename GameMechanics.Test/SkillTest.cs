using Csla;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillTest
  {
    [TestMethod]
    public void SingleAttributeBonus()
    {
      var c = DataPortal.Create<Character>("");
      c.Strength.Value = 7;
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
      var c = DataPortal.Create<Character>("");
      c.Strength.Value = 12;
      c.Endurance.Value = 10;
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
      Assert.AreEqual(9, s.AbilityScore);
    }

    [TestMethod]
    public void StandardSkills()
    {
      var c = DataPortal.Create<Character>("");
      Assert.AreEqual(8, c.Skills.Count);
    }
  }
}
