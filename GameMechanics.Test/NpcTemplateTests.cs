using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class NpcTemplateTests : TestBase
{
    [TestMethod]
    public async Task NpcTemplateList_Fetch_ReturnsOnlyTemplates()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var templateListPortal = provider.GetRequiredService<IDataPortal<NpcTemplateList>>();

        // Create a regular PC (should NOT appear in template list)
        var pc = dp.Create(42);
        pc.Name = "NpcTest_PC";
        pc.Species = "Human";
        pc.IsNpc = false;
        pc.IsTemplate = false;
        pc = await pc.SaveAsync();

        // Create an NPC that is NOT a template (should NOT appear)
        var npc = dp.Create(42);
        npc.Name = "NpcTest_ActiveNPC";
        npc.Species = "Goblin";
        npc.IsNpc = true;
        npc.IsTemplate = false;
        npc = await npc.SaveAsync();

        // Create an NPC template (SHOULD appear)
        var template = dp.Create(42);
        template.Name = "NpcTest_GoblinTemplate";
        template.Species = "Goblin";
        template.IsNpc = true;
        template.IsTemplate = true;
        template = await template.SaveAsync();

        // Fetch template list
        var templateList = await templateListPortal.FetchAsync();

        // Verify only the template is in the list
        Assert.IsTrue(templateList.Any(t => t.Name == "NpcTest_GoblinTemplate"),
            "Template should be in results");
        Assert.IsFalse(templateList.Any(t => t.Name == "NpcTest_PC"),
            "PC should not be in results");
        Assert.IsFalse(templateList.Any(t => t.Name == "NpcTest_ActiveNPC"),
            "Non-template NPC should not be in results");
    }

    [TestMethod]
    public async Task NpcTemplateInfo_LoadsAllProperties()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var templateListPortal = provider.GetRequiredService<IDataPortal<NpcTemplateList>>();

        // Create a template with all properties set
        var template = dp.Create(42);
        template.Name = "NpcTest_BossGoblin";
        template.Species = "Goblin";
        template.IsNpc = true;
        template.IsTemplate = true;
        template.Category = "Beasts";
        template.Tags = "undead,boss";
        template.DefaultDisposition = NpcDisposition.Hostile;
        template.CalculateDifficultyRating(); // Sets DifficultyRating based on skills
        template.VisibleToPlayers = true; // Active template

        template = await template.SaveAsync();

        // Fetch template list and find our template
        var templateList = await templateListPortal.FetchAsync();
        var info = templateList.First(t => t.Name == "NpcTest_BossGoblin");

        // Verify all properties loaded correctly
        Assert.AreEqual(template.Id, info.Id, "Id should match");
        Assert.AreEqual("NpcTest_BossGoblin", info.Name, "Name should match");
        Assert.AreEqual("Goblin", info.Species, "Species should match");
        Assert.AreEqual("Beasts", info.Category, "Category should match");
        Assert.AreEqual("undead,boss", info.Tags, "Tags should match");
        Assert.AreEqual(NpcDisposition.Hostile, info.DefaultDisposition, "DefaultDisposition should match");
        Assert.AreEqual(template.DifficultyRating, info.DifficultyRating, "DifficultyRating should match");
        Assert.IsTrue(info.IsActive, "IsActive should be true");
    }

    [TestMethod]
    public async Task NpcTemplateInfo_TagList_SplitsCorrectly()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var templateListPortal = provider.GetRequiredService<IDataPortal<NpcTemplateList>>();

        // Create template with multiple tags including whitespace
        var template = dp.Create(42);
        template.Name = "NpcTest_TagSplitTest";
        template.Species = "Orc";
        template.IsNpc = true;
        template.IsTemplate = true;
        template.Tags = "melee, ranged, boss"; // Note spaces after commas

        template = await template.SaveAsync();

        // Fetch and get the template
        var templateList = await templateListPortal.FetchAsync();
        var info = templateList.First(t => t.Name == "NpcTest_TagSplitTest");

        // Verify TagList parses correctly with trimming
        var tagList = info.TagList.ToList();
        Assert.AreEqual(3, tagList.Count, "Should have 3 tags");
        Assert.IsTrue(tagList.Contains("melee"), "Should contain 'melee'");
        Assert.IsTrue(tagList.Contains("ranged"), "Should contain 'ranged'");
        Assert.IsTrue(tagList.Contains("boss"), "Should contain 'boss'");
        // Verify whitespace was trimmed
        Assert.IsFalse(tagList.Any(t => t.StartsWith(" ") || t.EndsWith(" ")),
            "Tags should be trimmed");
    }

    [TestMethod]
    public async Task NpcTemplateInfo_TagList_HandlesEmpty()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var templateListPortal = provider.GetRequiredService<IDataPortal<NpcTemplateList>>();

        // Create template with null tags
        var template = dp.Create(42);
        template.Name = "NpcTest_NullTagsTest";
        template.Species = "Human";
        template.IsNpc = true;
        template.IsTemplate = true;
        template.Tags = null;

        template = await template.SaveAsync();

        // Fetch and get the template
        var templateList = await templateListPortal.FetchAsync();
        var info = templateList.First(t => t.Name == "NpcTest_NullTagsTest");

        // Verify TagList returns empty enumerable (not null)
        Assert.IsNotNull(info.TagList, "TagList should not be null");
        Assert.AreEqual(0, info.TagList.Count(), "TagList should be empty");
    }

    [TestMethod]
    public async Task NpcTemplateInfo_TagList_HandlesSingleTag()
    {
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var templateListPortal = provider.GetRequiredService<IDataPortal<NpcTemplateList>>();

        // Create template with single tag
        var template = dp.Create(42);
        template.Name = "NpcTest_SingleTagTest";
        template.Species = "Elf";
        template.IsNpc = true;
        template.IsTemplate = true;
        template.Tags = "solo";

        template = await template.SaveAsync();

        // Fetch and get the template
        var templateList = await templateListPortal.FetchAsync();
        var info = templateList.First(t => t.Name == "NpcTest_SingleTagTest");

        // Verify TagList contains exactly one tag
        var tagList = info.TagList.ToList();
        Assert.AreEqual(1, tagList.Count, "Should have exactly 1 tag");
        Assert.AreEqual("solo", tagList[0], "Tag should be 'solo'");
    }
}
