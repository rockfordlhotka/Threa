using Csla;
using Csla.Configuration;
using GameMechanics.Reference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test
{
  [TestClass]
  public class CharacterTests
  {
    private ServiceProvider InitServices()
    {
      IServiceCollection services = new ServiceCollection();
      services.AddCsla();
      return services.BuildServiceProvider();
    }

    [TestMethod]
    public void CheckHealth()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      var str = c.GetAttribute("STR");
      var end = c.GetAttribute("END");
      var wil = c.GetAttribute("WIL");
      Assert.AreEqual(str * 2 - 5, c.Vitality.Value, "vit");
      Assert.AreEqual(c.Vitality.Value, c.Vitality.BaseValue, "vitbase");
      Assert.AreEqual((end + wil) / 2 - 5, c.Fatigue.Value, "fat");
      Assert.AreEqual(c.Fatigue.Value, c.Fatigue.BaseValue, "fatbase");
    }

    [TestMethod]
    public void TakeDamage()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      var dmg = rv.CalculateDamageValue(0, 1);
      var result = DamageSheet.GetDamageResult(dmg.Damage);
      c.TakeDamage(dmg);
      Assert.AreEqual(result.Fatigue, c.Fatigue.PendingDamage);
      Assert.AreEqual(result.Vitality, c.Vitality.PendingDamage);
    }

    [TestMethod]
    public void HealFatigue()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      DamageValue dmg = new DamageValue(rv, 5, 1);
      while (dmg.Damage < 1 || dmg.Damage > 6)
        dmg = new DamageValue(rv, 5, 1);

      // use damage value
      c.TakeDamage(dmg);
      c.EndOfRound();
      Assert.IsTrue(c.Fatigue.BaseValue > c.Fatigue.Value, $"no initial damage {dmg.Damage}");
      while (c.Fatigue.Value < c.Fatigue.BaseValue)
        c.EndOfRound();
      Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper healing");
      c.EndOfRound();
      Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper noop");
    }

    [TestMethod]
    public void HealVitality()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      // get a positive damage value
      DamageValue dmg;
      do
      {
        dmg = rv.CalculateDamageValue(7, 1);
      } while (dmg.Damage < 7);

      // use damage value
      c.TakeDamage(dmg);
      c.EndOfRound();
      Assert.IsTrue(c.Vitality.BaseValue > c.Vitality.Value, $"no initial damage {dmg.Damage}");
      while (c.Vitality.Value < c.Vitality.BaseValue)
        c.EndOfRound();
      Assert.AreEqual(c.Vitality.BaseValue, c.Vitality.Value, "improper healing");
      c.EndOfRound();
      Assert.AreEqual(c.Vitality.BaseValue, c.Vitality.Value, "improper noop");
    }
  }
}
