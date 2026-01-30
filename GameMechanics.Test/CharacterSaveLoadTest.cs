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
  }
}

