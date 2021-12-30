using Csla;
using Csla.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillTest
  {
    private ServiceProvider InitServices()
    {
      IServiceCollection services = new ServiceCollection();
      services.AddCsla();
      return services.BuildServiceProvider();
    }

    [TestMethod]
    public void SingleAttributeBonus()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 7;
      var cs = new CharacterSkill
      {
        PrimaryAttribute = "STR",
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      Assert.AreEqual("STR", s.PrimaryAttribute);
      Assert.AreEqual(3, s.Level);
      Assert.AreEqual(-2, s.Bonus);
      Assert.AreEqual(5, s.AbilityScore);
    }

    [TestMethod]
    public void MultiAttributeBonus()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 12;
      c.AttributeList.Where(r => r.Name == "END").First().Value = 10;
      var cs = new CharacterSkill
      {
        PrimaryAttribute = "STR/END",
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      Assert.AreEqual("STR/END", s.PrimaryAttribute);
      Assert.AreEqual(3, s.Level);
      Assert.AreEqual(-2, s.Bonus);
      Assert.AreEqual(9, s.AbilityScore);
    }

    [TestMethod]
    public void StandardSkills()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      Assert.AreEqual(8, c.Skills.Count);
    }
  }
}
