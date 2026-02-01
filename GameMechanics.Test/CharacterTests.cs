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
  public class CharacterTests : TestBase
  {

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
      var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      var dmg = rv.CalculateDamageValue(0, 1);
      var result = DamageSheet.GetDamageResult(dmg.Damage);
      c.TakeDamage(dmg, effectPortal);
      Assert.AreEqual(result.Fatigue, c.Fatigue.PendingDamage);
      Assert.AreEqual(result.Vitality, c.Vitality.PendingDamage);
    }

    [TestMethod]
    public void HealFatigue()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      DamageValue dmg = new DamageValue(rv, 5, 1);
      while (dmg.Damage < 1 || dmg.Damage > 6)
        dmg = new DamageValue(rv, 5, 1);

      // use damage value
      c.TakeDamage(dmg, effectPortal);
      c.EndOfRound(effectPortal);
      Assert.IsTrue(c.Fatigue.BaseValue > c.Fatigue.Value, $"no initial damage {dmg.Damage}");
      while (c.Fatigue.Value < c.Fatigue.BaseValue)
        c.EndOfRound(effectPortal);
      Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper healing");
      c.EndOfRound(effectPortal);
      Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper noop");
    }

    [TestMethod]
    public void HealVitality()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
      var c = dp.Create(42);
      var rv = ResultValues.GetResult(5);
      // get a positive damage value
      DamageValue dmg;
      do
      {
        dmg = rv.CalculateDamageValue(7, 1);
      } while (dmg.Damage < 7);

      // use damage value
      c.TakeDamage(dmg, effectPortal);

      // Wait for all pending damage to be applied first
      int maxIterations = 100; // prevent infinite loop
      int iterations = 0;
      while (c.Vitality.PendingDamage > 0 && iterations < maxIterations)
      {
        c.EndOfRound(effectPortal);
        iterations++;
      }
      Assert.IsTrue(c.Vitality.BaseValue > c.Vitality.Value, $"no initial damage {dmg.Damage}");

      // Unlike Fatigue, Vitality does NOT have passive healing in EndOfRound.
      // VIT healing requires PendingHealing to be set externally (spells, potions, hourly recovery).
      // Set PendingHealing to simulate receiving healing.
      c.Vitality.PendingHealing = c.Vitality.BaseValue - c.Vitality.Value;

      iterations = 0;
      while (c.Vitality.Value < c.Vitality.BaseValue && iterations < maxIterations)
      {
        c.EndOfRound(effectPortal);
        iterations++;
      }
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
      
      // Verify all 8 attributes exist (STR, DEX, END, INT, ITT, WIL, PHY, SOC)
      Assert.AreEqual(8, c.AttributeList.Count);
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
      
      // Per design: Orc has STR +2, END +1, INT -1, SOC -1
      Assert.AreEqual(2, orc.GetModifier("STR"), "Orc STR modifier");
      Assert.AreEqual(1, orc.GetModifier("END"), "Orc END modifier");
      Assert.AreEqual(-1, orc.GetModifier("INT"), "Orc INT modifier");
      Assert.AreEqual(-1, orc.GetModifier("SOC"), "Orc SOC modifier");
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

    [TestMethod]
    public async Task CharacterSpeciesChange_UpdatesAttributeModifiers()
    {
      var provider = InitServices();
      var characterPortal = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      
      // Create character as Human (no modifiers)
      var character = characterPortal.Create(42);
      character.Name = "Test Character";
      
      // Record base values
      var strBaseValue = character.AttributeList.First(a => a.Name == "STR").BaseValue;
      var intBaseValue = character.AttributeList.First(a => a.Name == "INT").BaseValue;
      
      // Initial values should equal base values for Human (no modifiers)
      Assert.AreEqual(strBaseValue, character.AttributeList.First(a => a.Name == "STR").Value);
      Assert.AreEqual(intBaseValue, character.AttributeList.First(a => a.Name == "INT").Value);
      
      // Change species to Elf (INT +1, STR -1)
      var speciesList = await speciesListPortal.FetchAsync();
      var elfSpecies = speciesList.First(s => s.Id == "Elf");
      character.Species = "Elf";
      character.UpdateSpeciesModifiers(elfSpecies);
      
      // Base values should remain unchanged
      Assert.AreEqual(strBaseValue, character.AttributeList.First(a => a.Name == "STR").BaseValue, "STR base value should not change");
      Assert.AreEqual(intBaseValue, character.AttributeList.First(a => a.Name == "INT").BaseValue, "INT base value should not change");
      
      // Final values should reflect modifiers
      Assert.AreEqual(strBaseValue - 1, character.AttributeList.First(a => a.Name == "STR").Value, "STR final value with Elf modifier");
      Assert.AreEqual(intBaseValue + 1, character.AttributeList.First(a => a.Name == "INT").Value, "INT final value with Elf modifier");
      
      // Species modifiers should be set correctly
      Assert.AreEqual(-1, character.AttributeList.First(a => a.Name == "STR").SpeciesModifier, "STR modifier");
      Assert.AreEqual(1, character.AttributeList.First(a => a.Name == "INT").SpeciesModifier, "INT modifier");
      
      // Change species to Orc (STR +2, END +1, INT -1, PHY -1)
      var orcSpecies = speciesList.First(s => s.Id == "Orc");
      character.Species = "Orc";
      character.UpdateSpeciesModifiers(orcSpecies);
      
      // Base values should still remain unchanged
      Assert.AreEqual(strBaseValue, character.AttributeList.First(a => a.Name == "STR").BaseValue, "STR base value should not change after second species change");
      Assert.AreEqual(intBaseValue, character.AttributeList.First(a => a.Name == "INT").BaseValue, "INT base value should not change after second species change");
      
      // Final values should reflect new modifiers
      Assert.AreEqual(strBaseValue + 2, character.AttributeList.First(a => a.Name == "STR").Value, "STR final value with Orc modifier");
      Assert.AreEqual(intBaseValue - 1, character.AttributeList.First(a => a.Name == "INT").Value, "INT final value with Orc modifier");
      
      // Species modifiers should be updated
      Assert.AreEqual(2, character.AttributeList.First(a => a.Name == "STR").SpeciesModifier, "STR modifier for Orc");
      Assert.AreEqual(-1, character.AttributeList.First(a => a.Name == "INT").SpeciesModifier, "INT modifier for Orc");
    }

    [TestMethod]
    public async Task CharacterSpeciesChange_PersistsCorrectly()
    {
      var provider = InitServices();
      var characterPortal = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
      
      // Create character as Human
      var character = characterPortal.Create(42);
      character.Name = "Species Persistence Test";
      
      // Save initial character
      character = await character.SaveAsync();
      var characterId = character.Id;
      
      // Record base values
      var strBaseValue = character.AttributeList.First(a => a.Name == "STR").BaseValue;
      var intBaseValue = character.AttributeList.First(a => a.Name == "INT").BaseValue;
      
      // Change species to Elf and save
      var speciesList = await speciesListPortal.FetchAsync();
      var elfSpecies = speciesList.First(s => s.Id == "Elf");
      character.Species = "Elf";
      character.UpdateSpeciesModifiers(elfSpecies);
      character = await character.SaveAsync();
      
      // Fetch the character again
      var fetched = await characterPortal.FetchAsync(characterId);
      
      // Verify species is persisted
      Assert.AreEqual("Elf", fetched.Species);
      
      // Verify base values are unchanged
      Assert.AreEqual(strBaseValue, fetched.AttributeList.First(a => a.Name == "STR").BaseValue, "Fetched STR base value");
      Assert.AreEqual(intBaseValue, fetched.AttributeList.First(a => a.Name == "INT").BaseValue, "Fetched INT base value");
      
      // Verify final values include species modifiers
      Assert.AreEqual(strBaseValue - 1, fetched.AttributeList.First(a => a.Name == "STR").Value, "Fetched STR final value");
      Assert.AreEqual(intBaseValue + 1, fetched.AttributeList.First(a => a.Name == "INT").Value, "Fetched INT final value");
      
      // Verify species modifiers are correct
      Assert.AreEqual(-1, fetched.AttributeList.First(a => a.Name == "STR").SpeciesModifier, "Fetched STR modifier");
      Assert.AreEqual(1, fetched.AttributeList.First(a => a.Name == "INT").SpeciesModifier, "Fetched INT modifier");
    }

    [TestMethod]
    public void CharacterCurrency_CopperCoins_CannotBeNegative()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Attempt to set negative copper coins
      c.CopperCoins = -1;

      // Validation should fail
      Assert.IsFalse(c.IsValid, "Character should be invalid with negative copper coins");
      Assert.IsTrue(c.BrokenRulesCollection.ErrorCount > 0, "Should have broken rules");
    }

    [TestMethod]
    public void CharacterCurrency_SilverCoins_CannotBeNegative()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Attempt to set negative silver coins
      c.SilverCoins = -5;

      // Validation should fail
      Assert.IsFalse(c.IsValid, "Character should be invalid with negative silver coins");
      Assert.IsTrue(c.BrokenRulesCollection.ErrorCount > 0, "Should have broken rules");
    }

    [TestMethod]
    public void CharacterCurrency_GoldCoins_CannotBeNegative()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Attempt to set negative gold coins
      c.GoldCoins = -10;

      // Validation should fail
      Assert.IsFalse(c.IsValid, "Character should be invalid with negative gold coins");
      Assert.IsTrue(c.BrokenRulesCollection.ErrorCount > 0, "Should have broken rules");
    }

    [TestMethod]
    public void CharacterCurrency_PlatinumCoins_CannotBeNegative()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Attempt to set negative platinum coins
      c.PlatinumCoins = -2;

      // Validation should fail
      Assert.IsFalse(c.IsValid, "Character should be invalid with negative platinum coins");
      Assert.IsTrue(c.BrokenRulesCollection.ErrorCount > 0, "Should have broken rules");
    }

    [TestMethod]
    public void CharacterCurrency_ZeroValues_AreValid()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Set all currency to zero (should be valid)
      c.CopperCoins = 0;
      c.SilverCoins = 0;
      c.GoldCoins = 0;
      c.PlatinumCoins = 0;

      // Should be valid - no currency-related broken rules
      var currencyRules = c.BrokenRulesCollection.Where(r => 
        r.Property == "CopperCoins" || r.Property == "SilverCoins" || 
        r.Property == "GoldCoins" || r.Property == "PlatinumCoins");
      Assert.IsFalse(currencyRules.Any(), 
                    "Character should be valid with zero currency values");
    }

    [TestMethod]
    public void CharacterCurrency_PositiveValues_AreValid()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var c = dp.Create(42);

      // Set positive currency values
      c.CopperCoins = 100;
      c.SilverCoins = 50;
      c.GoldCoins = 10;
      c.PlatinumCoins = 5;

      // Should be valid - no currency-related broken rules
      var currencyRules = c.BrokenRulesCollection.Where(r => 
        r.Property == "CopperCoins" || r.Property == "SilverCoins" || 
        r.Property == "GoldCoins" || r.Property == "PlatinumCoins");
      Assert.IsFalse(currencyRules.Any(), 
                    "Character should be valid with positive currency values");
    }
  }
}
