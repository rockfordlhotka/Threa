using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameMechanics.Test;

[TestClass]
public class CharacterSettingTests : TestBase
{
    [TestMethod]
    public void GameSettings_IsValid_ReturnsTrue_ForFantasy()
    {
        Assert.IsTrue(GameSettings.IsValid("fantasy"));
    }

    [TestMethod]
    public void GameSettings_IsValid_ReturnsTrue_ForSciFi()
    {
        Assert.IsTrue(GameSettings.IsValid("scifi"));
    }

    [TestMethod]
    public void GameSettings_IsValid_ReturnsFalse_ForNull()
    {
        Assert.IsFalse(GameSettings.IsValid(null));
    }

    [TestMethod]
    public void GameSettings_IsValid_ReturnsFalse_ForEmpty()
    {
        Assert.IsFalse(GameSettings.IsValid(""));
    }

    [TestMethod]
    public void GameSettings_IsValid_ReturnsFalse_ForUnknown()
    {
        Assert.IsFalse(GameSettings.IsValid("steampunk"));
    }

    [TestMethod]
    public void GameSettings_DisplayName_Fantasy()
    {
        Assert.AreEqual("Fantasy (Arcanum)", GameSettings.DisplayName("fantasy"));
    }

    [TestMethod]
    public void GameSettings_DisplayName_SciFi()
    {
        Assert.AreEqual("Sci-Fi (Neon Circuit)", GameSettings.DisplayName("scifi"));
    }

    [TestMethod]
    public void GameSettings_Default_IsFantasy()
    {
        Assert.AreEqual("fantasy", GameSettings.Default);
    }

    [TestMethod]
    public void CharacterEdit_Create_DefaultSettingIsFantasy()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();

        var character = dp.Create(42);

        Assert.AreEqual(GameSettings.Fantasy, character.Setting);
    }

    [TestMethod]
    public void CharacterEdit_Setting_CanBeChanged()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();

        var character = dp.Create(42);
        character.Setting = GameSettings.SciFi;

        Assert.AreEqual(GameSettings.SciFi, character.Setting);
    }

    [TestMethod]
    public async Task CharacterEdit_Setting_PersistsThroughSaveFetch()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();

        var character = dp.Create(42);
        character.Name = "Test SciFi Character";
        character.Setting = GameSettings.SciFi;
        character = await character.SaveAsync();

        var fetched = await dp.FetchAsync(character.Id);

        Assert.AreEqual(GameSettings.SciFi, fetched.Setting);
    }

    [TestMethod]
    public async Task CharacterEdit_Setting_DefaultsToFantasyForExistingRecords()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();

        // Create and save with default setting
        var character = dp.Create(42);
        character.Name = "Default Setting Character";
        character = await character.SaveAsync();

        var fetched = await dp.FetchAsync(character.Id);

        Assert.AreEqual(GameSettings.Fantasy, fetched.Setting);
    }

    [TestMethod]
    public void CharacterDto_Setting_DefaultsToFantasy()
    {
        var dto = new Threa.Dal.Dto.Character();
        Assert.AreEqual("fantasy", dto.Setting);
    }

    [TestMethod]
    public async Task NpcSpawner_PropagatesSetting_FromTemplate()
    {
        var provider = InitServices();
        var characterDal = provider.GetRequiredService<Threa.Dal.ICharacterDal>();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();

        // Create a sci-fi template and save it
        var template = dp.Create(42);
        template.Name = "Cyber Drone";
        template.IsNpc = true;
        template.IsTemplate = true;
        template.Setting = GameSettings.SciFi;
        template = await template.SaveAsync();

        // Verify template has sci-fi setting persisted
        var templateDto = await characterDal.GetCharacterAsync(template.Id);
        Assert.AreEqual(GameSettings.SciFi, templateDto.Setting);

        // Simulate what NpcSpawner does: copy template fields including Setting
        var npcDto = characterDal.GetBlank();
        npcDto.Name = "Cyber Drone #1";
        npcDto.Species = templateDto.Species;
        npcDto.Setting = templateDto.Setting;
        npcDto.IsNpc = true;
        npcDto.IsTemplate = false;
        npcDto.IsPlayable = true;
        npcDto.PlayerId = templateDto.PlayerId;
        npcDto.DamageClass = templateDto.DamageClass;
        var saved = await characterDal.SaveCharacterAsync(npcDto);

        // Verify the spawned NPC has the sci-fi setting
        var fetched = await dp.FetchAsync(saved.Id);
        Assert.AreEqual(GameSettings.SciFi, fetched.Setting);
    }
}
