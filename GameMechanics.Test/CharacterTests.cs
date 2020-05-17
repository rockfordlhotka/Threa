using Csla;
using GameMechanics.Reference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test
{
  [TestClass]
  public class CharacterTests
  {
    [TestMethod]
    public void CheckHealth()
    {
      var c = DataPortal.Create<Character>();
      var str = c.GetAttribute("STR");
      var end = c.GetAttribute("END");
      var wil = c.GetAttribute("WIL");
      Assert.AreEqual(str * 2 - 5, c.Vitality.Value);
      Assert.AreEqual(c.Vitality.Value, c.Vitality.BaseValue);
      Assert.AreEqual(end + wil - 5, c.Fatigue.Value);
      Assert.AreEqual(c.Fatigue.Value, c.Fatigue.BaseValue);
    }

    [TestMethod]
    public void TakeDamage()
    {
      var c = DataPortal.Create<Character>();
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
      var c = DataPortal.Create<Character>();
      var rv = ResultValues.GetResult(5);
      // get a positive damage value
      DamageValue dmg;
      do
      {
        dmg = rv.CalculateDamageValue(0, 1);
      } while (dmg.Damage < 4);

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
      var c = DataPortal.Create<Character>();
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
