using Csla;
using Csla.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Test
{
  [TestClass]
  public class CharacterSaveLoadTest : TestBase
  {

    [TestMethod]
    public async Task Character_SaveAndLoad_AllPropertiesPersist()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      
      // Create a new character
      var character = dp.Create(42);
      character.Name = "Test Character";
      character.TrueName = "True Name Test";
      character.Aliases = "Alias1, Alias2";
      character.Species = "Elf";
      character.Height = "6'2\"";
      character.Weight = "180 lbs";
      character.SkinDescription = "Pale";
      character.HairDescription = "Silver";
      character.Description = "A tall elf warrior";
      
      // Modify an attribute
      var strAttr = character.AttributeList.First(a => a.Name == "STR");
      strAttr.BaseValue = 12;
      
      // Modify a skill (using a standard skill that always exists)
      var skill = character.Skills.FirstOrDefault(s => s.Name == "Physicality");
      if (skill != null)
      {
        skill.Level = 5;
        skill.XPBanked = 10;
      }
      
      // Save the character
      character = await character.SaveAsync();
      var characterId = character.Id;
      
      Assert.IsTrue(characterId > 0, "Character should have an ID after save");
      
      // Fetch the character
      var loaded = await dp.FetchAsync(characterId);
      
      // Verify all properties
      Assert.AreEqual("Test Character", loaded.Name, "Name should persist");
      Assert.AreEqual("True Name Test", loaded.TrueName, "TrueName should persist");
      Assert.AreEqual("Alias1, Alias2", loaded.Aliases, "Aliases should persist");
      Assert.AreEqual("Elf", loaded.Species, "Species should persist");
      Assert.AreEqual("6'2\"", loaded.Height, "Height should persist");
      Assert.AreEqual("180 lbs", loaded.Weight, "Weight should persist");
      Assert.AreEqual("Pale", loaded.SkinDescription, "SkinDescription should persist");
      Assert.AreEqual("Silver", loaded.HairDescription, "HairDescription should persist");
      Assert.AreEqual("A tall elf warrior", loaded.Description, "Description should persist");
      
      // Verify attribute
      var loadedStrAttr = loaded.AttributeList.First(a => a.Name == "STR");
      Assert.AreEqual(12, loadedStrAttr.BaseValue, "STR BaseValue should persist");
      
      // Verify skill
      var loadedSkill = loaded.Skills.FirstOrDefault(s => s.Name == "Physicality");
      Assert.IsNotNull(loadedSkill, "Physicality skill should exist");
      Assert.AreEqual(5, loadedSkill.Level, "Skill level should persist");
      Assert.AreEqual(10, loadedSkill.XPBanked, "Skill XPBanked should persist");
    }

    [TestMethod]
    public async Task Character_UpdateAndReload_ChangesPersist()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      
      // Create and save a character
      var character = dp.Create(42);
      character.Name = "Original Name";
      character.Species = "Human";
      
      var skill = character.Skills.FirstOrDefault(s => s.Name == "Physicality");
      if (skill != null)
      {
        skill.Level = 3;
      }
      
      character = await character.SaveAsync();
      var characterId = character.Id;
      
      // Fetch and modify
      var fetched = await dp.FetchAsync(characterId);
      fetched.Name = "Updated Name";
      fetched.Species = "Dwarf";
      
      var fetchedSkill = fetched.Skills.FirstOrDefault(s => s.Name == "Physicality");
      if (fetchedSkill != null)
      {
        fetchedSkill.Level = 7;
      }
      
      // Save again
      fetched = await fetched.SaveAsync();
      
      // Fetch again and verify
      var reloaded = await dp.FetchAsync(characterId);
      Assert.AreEqual("Updated Name", reloaded.Name, "Updated name should persist");
      Assert.AreEqual("Dwarf", reloaded.Species, "Updated species should persist");
      
      var reloadedSkill = reloaded.Skills.FirstOrDefault(s => s.Name == "Physicality");
      Assert.IsNotNull(reloadedSkill, "Physicality skill should still exist");
      Assert.AreEqual(7, reloadedSkill.Level, "Updated skill level should persist");
    }

    [TestMethod]
    public async Task Character_XPBanked_GMAdditionMergedWithPlayerSave()
    {
      // This tests the concurrency fix for issue #26:
      // When a player is editing a character and the GM adds XP to their bank,
      // the player's save should not overwrite the GM's addition.

      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var dal = provider.GetRequiredService<ICharacterDal>();

      // Create and save a character with initial XPBanked
      var character = dp.Create(42);
      character.Name = "XP Concurrency Test";
      character.XPBanked = 100;

      character = await character.SaveAsync();
      var characterId = character.Id;

      // Player fetches the character (simulating player loading the character sheet)
      var playerCopy = await dp.FetchAsync(characterId);
      Assert.AreEqual(100, playerCopy.XPBanked, "Player should see initial XPBanked");

      // GM adds XP directly to the database (simulating GM's XP grant)
      var dbCharacter = await dal.GetCharacterAsync(characterId);
      dbCharacter.XPBanked = 150;  // GM added 50 XP
      await dal.SaveCharacterAsync(dbCharacter);

      // Player makes an unrelated change and saves
      playerCopy.Name = "Updated Name";

      // Save should merge GM's XP addition with player's (unchanged) XPBanked
      playerCopy = await playerCopy.SaveAsync();

      // Verify the XPBanked includes GM's addition
      var reloaded = await dp.FetchAsync(characterId);
      Assert.AreEqual(150, reloaded.XPBanked, "XPBanked should include GM's 50 XP addition");
      Assert.AreEqual("Updated Name", reloaded.Name, "Player's name change should persist");
    }

    [TestMethod]
    public async Task Character_XPBanked_PlayerSpendingPreservedWithGMAddition()
    {
      // Tests that when both player spends XP AND GM adds XP, both changes are preserved

      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var dal = provider.GetRequiredService<ICharacterDal>();

      // Create and save a character with initial XPBanked
      var character = dp.Create(42);
      character.Name = "XP Merge Test";
      character.XPBanked = 100;

      character = await character.SaveAsync();
      var characterId = character.Id;

      // Player fetches the character
      var playerCopy = await dp.FetchAsync(characterId);

      // Player spends 30 XP (reduces XPBanked from 100 to 70)
      playerCopy.SpendXP(30);
      Assert.AreEqual(70, playerCopy.XPBanked, "Player should have 70 XP after spending 30");

      // GM adds XP directly to the database (simulating GM's XP grant)
      var dbCharacter = await dal.GetCharacterAsync(characterId);
      dbCharacter.XPBanked = 150;  // GM added 50 XP
      await dal.SaveCharacterAsync(dbCharacter);

      // Player saves their character
      playerCopy = await playerCopy.SaveAsync();

      // Verify both changes are merged:
      // - Original: 100
      // - Player spent: -30 (local copy becomes 70)
      // - GM added: +50 (database becomes 150)
      // - Final: 70 + (150 - 100) = 120
      var reloaded = await dp.FetchAsync(characterId);
      Assert.AreEqual(120, reloaded.XPBanked, "XPBanked should be 120 (player spent 30, GM added 50)");
    }
  }
}

