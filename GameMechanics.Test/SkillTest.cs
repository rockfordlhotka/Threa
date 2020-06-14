using Csla;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 7;
      var cs = new CharacterSkill
      {
        PrimaryAttribute = "STR",
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
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 12;
      c.AttributeList.Where(r => r.Name == "END").First().Value = 10;
      var cs = new CharacterSkill
      {
        PrimaryAttribute = "STR/END",
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
