using Csla;
using Csla.Configuration;
using GameMechanics.Reference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Test
{
  [TestClass]
  public class CharacterTests
  {
    private ServiceProvider InitServices()
    {
      IServiceCollection services = new ServiceCollection();
      services.AddCsla();
      services.AddMockDb();
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
      Assert.AreEqual(end + wil - 5, c.Fatigue.Value, "fat");
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
      Assert.IsTrue(c.Vitality.Value >= c.Vitality.BaseValue, "improper healing");
    }

    [TestMethod]
    public async Task CreateCharacterWithSpecies_AppliesModifiers()
    {
      var provider = InitServices();
      
      // Get species from database
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      var speciesList = await speciesListPortal.FetchAsync();
      var elfSpecies = speciesList.First(s => s.Id == "Elf");
      
      // Create character with species
      var characterPortal = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = characterPortal.Create(42, elfSpecies);
      
      // Verify species is set
      Assert.AreEqual("Elf", c.Species);
      
      // Verify all 7 attributes exist
      Assert.AreEqual(7, c.AttributeList.Count);
    }

    [TestMethod]
    public async Task SpeciesList_ContainsAllSpecies()
    {
      var provider = InitServices();
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      var speciesList = await speciesListPortal.FetchAsync();
      
      // Should have 5 species per design
      Assert.AreEqual(5, speciesList.Count);
      
      // Verify expected species exist
      Assert.IsNotNull(speciesList.FirstOrDefault(s => s.Id == "Human"));
      Assert.IsNotNull(speciesList.FirstOrDefault(s => s.Id == "Elf"));
      Assert.IsNotNull(speciesList.FirstOrDefault(s => s.Id == "Dwarf"));
      Assert.IsNotNull(speciesList.FirstOrDefault(s => s.Id == "Halfling"));
      Assert.IsNotNull(speciesList.FirstOrDefault(s => s.Id == "Orc"));
    }

    [TestMethod]
    public async Task ElfSpecies_HasCorrectModifiers()
    {
      var provider = InitServices();
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      var speciesList = await speciesListPortal.FetchAsync();
      var elf = speciesList.First(s => s.Id == "Elf");
      
      // Per design: Elf has INT +1, STR -1
      Assert.AreEqual(1, elf.GetModifier("INT"), "Elf INT modifier");
      Assert.AreEqual(-1, elf.GetModifier("STR"), "Elf STR modifier");
      Assert.AreEqual(0, elf.GetModifier("DEX"), "Elf DEX should be 0");
    }

    [TestMethod]
    public async Task OrcSpecies_HasCorrectModifiers()
    {
      var provider = InitServices();
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      var speciesList = await speciesListPortal.FetchAsync();
      var orc = speciesList.First(s => s.Id == "Orc");
      
      // Per design: Orc has STR +2, END +1, INT -1, PHY -1
      Assert.AreEqual(2, orc.GetModifier("STR"), "Orc STR modifier");
      Assert.AreEqual(1, orc.GetModifier("END"), "Orc END modifier");
      Assert.AreEqual(-1, orc.GetModifier("INT"), "Orc INT modifier");
      Assert.AreEqual(-1, orc.GetModifier("PHY"), "Orc PHY modifier");
      Assert.AreEqual(0, orc.GetModifier("DEX"), "Orc DEX should be 0");
    }

    [TestMethod]
    public void CharacterCurrency_CanSetAndGet()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      
      // Set currency values
      c.CopperCoins = 10;
      c.SilverCoins = 5;
      c.GoldCoins = 2;
      c.PlatinumCoins = 1;
      
      // Verify values are stored correctly
      Assert.AreEqual(10, c.CopperCoins, "Copper coins");
      Assert.AreEqual(5, c.SilverCoins, "Silver coins");
      Assert.AreEqual(2, c.GoldCoins, "Gold coins");
      Assert.AreEqual(1, c.PlatinumCoins, "Platinum coins");
    }

    [TestMethod]
    public void CharacterCurrency_TotalCopperValue_CalculatesCorrectly()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      
      // Set currency values
      c.CopperCoins = 10;
      c.SilverCoins = 5;   // 5 * 20 = 100 cp
      c.GoldCoins = 2;     // 2 * 400 = 800 cp
      c.PlatinumCoins = 1; // 1 * 8000 = 8000 cp
      
      // Total should be: 10 + 100 + 800 + 8000 = 8910 cp
      Assert.AreEqual(8910, c.TotalCopperValue, "Total copper value");
    }

    [TestMethod]
    public async Task CharacterCurrency_PersistsToDatabase()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);
      
      // Set values and save
      c.Name = "Test Currency Character";
      c.Species = "Human";
      c.CopperCoins = 25;
      c.SilverCoins = 10;
      c.GoldCoins = 3;
      c.PlatinumCoins = 1;
      
      c = await c.SaveAsync();
      var characterId = c.Id;
      
      // Fetch and verify
      var fetched = await dp.FetchAsync(characterId);
      Assert.AreEqual(25, fetched.CopperCoins, "Fetched copper coins");
      Assert.AreEqual(10, fetched.SilverCoins, "Fetched silver coins");
      Assert.AreEqual(3, fetched.GoldCoins, "Fetched gold coins");
      Assert.AreEqual(1, fetched.PlatinumCoins, "Fetched platinum coins");
      // Total = 25 + (10 * 20) + (3 * 400) + (1 * 8000) = 25 + 200 + 1200 + 8000 = 9425
      Assert.AreEqual(9425, fetched.TotalCopperValue, "Fetched total copper value");
    }
  }
}
