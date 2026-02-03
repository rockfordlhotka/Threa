using Csla;
using GameMechanics.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ItemEffectEditTests : TestBase
{
    #region ItemEffectEdit Child Object Tests

    [TestMethod]
    public async Task ItemEffectEdit_Create_InitializesDefaults()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Act
        var effect = effectPortal.CreateChild(0);

        // Assert
        Assert.AreEqual(0, effect.Id, "Id should be 0 for new effect");
        Assert.AreEqual(0, effect.ItemTemplateId, "ItemTemplateId should match the passed value");
        Assert.AreEqual(string.Empty, effect.Name, "Name should be empty string");
        Assert.AreEqual(EffectType.ItemEffect, effect.EffectType, "EffectType should default to ItemEffect");
        Assert.AreEqual(ItemEffectTrigger.WhileEquipped, effect.Trigger, "Trigger should default to WhileEquipped");
        Assert.IsFalse(effect.IsCursed, "IsCursed should default to false");
        Assert.IsFalse(effect.RequiresAttunement, "RequiresAttunement should default to false");
        Assert.IsNull(effect.DurationRounds, "DurationRounds should be null");
        Assert.IsTrue(string.IsNullOrEmpty(effect.BehaviorState), "BehaviorState should be null or empty");
        Assert.IsTrue(effect.IsActive, "IsActive should default to true");
        Assert.AreEqual(0, effect.Priority, "Priority should default to 0");
    }

    [TestMethod]
    public async Task ItemEffectEdit_Fetch_LoadsFromDto()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        var dto = new ItemEffectDefinition
        {
            Id = 42,
            ItemTemplateId = 10,
            EffectType = EffectType.Buff,
            Name = "Strength Boost",
            Description = "Grants +3 STR",
            Trigger = ItemEffectTrigger.WhileEquipped,
            IsCursed = false,
            RequiresAttunement = true,
            DurationRounds = null,
            BehaviorState = "{\"Modifiers\":[{\"Type\":0,\"Target\":\"STR\",\"Value\":3}]}",
            IconName = "shield",
            IsActive = true,
            Priority = 5
        };

        // Act
        var effect = effectPortal.FetchChild(dto);

        // Assert
        Assert.AreEqual(42, effect.Id, "Id should be loaded from DTO");
        Assert.AreEqual(10, effect.ItemTemplateId, "ItemTemplateId should be loaded from DTO");
        Assert.AreEqual(EffectType.Buff, effect.EffectType, "EffectType should be loaded from DTO");
        Assert.AreEqual("Strength Boost", effect.Name, "Name should be loaded from DTO");
        Assert.AreEqual("Grants +3 STR", effect.Description, "Description should be loaded from DTO");
        Assert.AreEqual(ItemEffectTrigger.WhileEquipped, effect.Trigger, "Trigger should be loaded from DTO");
        Assert.IsFalse(effect.IsCursed, "IsCursed should be loaded from DTO");
        Assert.IsTrue(effect.RequiresAttunement, "RequiresAttunement should be loaded from DTO");
        Assert.IsNull(effect.DurationRounds, "DurationRounds should be loaded from DTO");
        Assert.IsNotNull(effect.BehaviorState, "BehaviorState should be loaded from DTO");
        Assert.AreEqual("shield", effect.IconName, "IconName should be loaded from DTO");
        Assert.IsTrue(effect.IsActive, "IsActive should be loaded from DTO");
        Assert.AreEqual(5, effect.Priority, "Priority should be loaded from DTO");
    }

    [TestMethod]
    public async Task ItemEffectEdit_Validation_NameRequired()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Act - Create effect with empty name (default)
        var effect = effectPortal.CreateChild(0);
        // Leave name empty, but set a valid trigger
        effect.Trigger = ItemEffectTrigger.WhileEquipped;

        // Assert
        Assert.IsFalse(effect.IsValid, "Effect with empty Name should not be valid");
        Assert.IsTrue(effect.BrokenRulesCollection.Any(r => r.Property == "Name"),
            "BrokenRulesCollection should contain Name rule violation");
    }

    [TestMethod]
    public async Task ItemEffectEdit_Validation_TriggerRequired()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Act - Create effect with Trigger = None
        var effect = effectPortal.CreateChild(0);
        effect.Name = "Test Effect";
        effect.Trigger = ItemEffectTrigger.None;

        // Assert
        Assert.IsFalse(effect.IsValid, "Effect with Trigger=None should not be valid");
        Assert.IsTrue(effect.BrokenRulesCollection.Any(r => r.Property == "Trigger"),
            "BrokenRulesCollection should contain Trigger rule violation");
    }

    [TestMethod]
    public async Task ItemEffectEdit_Validation_ValidEffectIsValid()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Act - Create valid effect
        var effect = effectPortal.CreateChild(0);
        effect.Name = "Valid Effect";
        effect.Trigger = ItemEffectTrigger.WhileEquipped;

        // Assert
        Assert.IsTrue(effect.IsValid, "Effect with Name and valid Trigger should be valid");
        Assert.AreEqual(0, effect.BrokenRulesCollection.ErrorCount,
            "Effect should have no errors");
    }

    [TestMethod]
    public async Task ItemEffectEdit_AllPropertiesSettable()
    {
        // Arrange
        var provider = InitServices();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Act
        var effect = effectPortal.CreateChild(10);
        effect.Name = "Life Drain";
        effect.Description = "Drains health over time";
        effect.EffectType = EffectType.Debuff;
        effect.Trigger = ItemEffectTrigger.WhilePossessed;
        effect.IsCursed = true;
        effect.RequiresAttunement = false;
        effect.DurationRounds = 10;
        effect.BehaviorState = "{\"test\":\"data\"}";
        effect.IconName = "skull";
        effect.IsActive = true;
        effect.Priority = 3;

        // Assert - all properties can be read back
        Assert.AreEqual("Life Drain", effect.Name, "Name should be settable");
        Assert.AreEqual("Drains health over time", effect.Description, "Description should be settable");
        Assert.AreEqual(EffectType.Debuff, effect.EffectType, "EffectType should be settable");
        Assert.AreEqual(ItemEffectTrigger.WhilePossessed, effect.Trigger, "Trigger should be settable");
        Assert.IsTrue(effect.IsCursed, "IsCursed should be settable");
        Assert.IsFalse(effect.RequiresAttunement, "RequiresAttunement should be settable");
        Assert.AreEqual(10, effect.DurationRounds, "DurationRounds should be settable");
        Assert.AreEqual("{\"test\":\"data\"}", effect.BehaviorState, "BehaviorState should be settable");
        Assert.AreEqual("skull", effect.IconName, "IconName should be settable");
        Assert.IsTrue(effect.IsActive, "IsActive should be settable");
        Assert.AreEqual(3, effect.Priority, "Priority should be settable");
    }

    #endregion

    #region ItemEffectEditList Tests

    [TestMethod]
    public async Task ItemEffectEditList_Create_IsEmpty()
    {
        // Arrange
        var provider = InitServices();
        var listPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEditList>>();

        // Act
        var list = listPortal.CreateChild();

        // Assert
        Assert.AreEqual(0, list.Count, "New list should be empty");
    }

    [TestMethod]
    public async Task ItemEffectEditList_Fetch_LoadsEffects()
    {
        // Arrange
        var provider = InitServices();
        var listPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEditList>>();

        var effects = new System.Collections.Generic.List<ItemEffectDefinition>
        {
            new() { Id = 1, Name = "Effect 1", Trigger = ItemEffectTrigger.WhileEquipped },
            new() { Id = 2, Name = "Effect 2", Trigger = ItemEffectTrigger.OnUse }
        };

        // Act
        var list = listPortal.FetchChild(effects);

        // Assert
        Assert.AreEqual(2, list.Count, "List should contain 2 effects");
        Assert.AreEqual("Effect 1", list[0].Name, "First effect name should match");
        Assert.AreEqual("Effect 2", list[1].Name, "Second effect name should match");
    }

    [TestMethod]
    public async Task ItemEffectEditList_AddNewEffect_AddsToList()
    {
        // Arrange
        var provider = InitServices();
        var listPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEditList>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        var list = listPortal.CreateChild();

        // Act
        var newEffect = list.AddNewEffect(10, effectPortal);

        // Assert
        Assert.AreEqual(1, list.Count, "List should contain 1 effect");
        Assert.AreSame(newEffect, list[0], "The returned effect should be in the list");
        Assert.AreEqual(10, newEffect.ItemTemplateId, "Effect should have the correct ItemTemplateId");
    }

    [TestMethod]
    public async Task ItemEffectEditList_GetByTrigger_FiltersCorrectly()
    {
        // Arrange
        var provider = InitServices();
        var listPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEditList>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        var list = listPortal.CreateChild();
        var effect1 = list.AddNewEffect(10, effectPortal);
        effect1.Name = "Effect A";
        effect1.Trigger = ItemEffectTrigger.WhileEquipped;

        var effect2 = list.AddNewEffect(10, effectPortal);
        effect2.Name = "Effect B";
        effect2.Trigger = ItemEffectTrigger.OnUse;

        var effect3 = list.AddNewEffect(10, effectPortal);
        effect3.Name = "Effect C";
        effect3.Trigger = ItemEffectTrigger.WhileEquipped;

        // Act
        var equippedEffects = list.GetByTrigger(ItemEffectTrigger.WhileEquipped).ToList();
        var onUseEffects = list.GetByTrigger(ItemEffectTrigger.OnUse).ToList();

        // Assert
        Assert.AreEqual(2, equippedEffects.Count, "Should have 2 WhileEquipped effects");
        Assert.AreEqual(1, onUseEffects.Count, "Should have 1 OnUse effect");
        Assert.IsTrue(equippedEffects.All(e => e.Trigger == ItemEffectTrigger.WhileEquipped),
            "All filtered effects should have WhileEquipped trigger");
    }

    #endregion

    #region Integration Tests with ItemTemplateEdit

    [TestMethod]
    public async Task ItemTemplateEdit_Create_HasEmptyEffectsList()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();

        // Act
        var template = await dp.CreateAsync();

        // Assert
        Assert.IsNotNull(template.Effects, "Effects collection should not be null");
        Assert.AreEqual(0, template.Effects.Count, "New template should have empty effects list");
    }

    [TestMethod]
    public async Task ItemTemplateEdit_SaveAndFetch_EffectsPersist()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Create a new template with an effect
        var template = await dp.CreateAsync();
        template.Name = "Magic Ring";
        template.ItemType = ItemType.Jewelry;
        template.EquipmentSlot = EquipmentSlot.FingerLeft1;

        var effect = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect.Name = "Strength Boost";
        effect.Description = "Grants +3 STR while worn";
        effect.EffectType = EffectType.Buff;
        effect.Trigger = ItemEffectTrigger.WhileEquipped;
        effect.IsCursed = false;

        // Act - Save
        template = await template.SaveAsync();
        var savedId = template.Id;
        Assert.IsTrue(savedId > 0, "Saved template should have an Id > 0");

        // Fetch the saved template
        var loaded = await dp.FetchAsync(savedId);

        // Assert
        Assert.IsNotNull(loaded.Effects, "Loaded template should have Effects collection");
        Assert.AreEqual(1, loaded.Effects.Count, "Loaded template should have 1 effect");

        var loadedEffect = loaded.Effects[0];
        Assert.AreEqual("Strength Boost", loadedEffect.Name, "Effect name should persist");
        Assert.AreEqual("Grants +3 STR while worn", loadedEffect.Description, "Effect description should persist");
        Assert.AreEqual(EffectType.Buff, loadedEffect.EffectType, "Effect type should persist");
        Assert.AreEqual(ItemEffectTrigger.WhileEquipped, loadedEffect.Trigger, "Effect trigger should persist");
        Assert.IsFalse(loadedEffect.IsCursed, "Effect IsCursed should persist");
    }

    [TestMethod]
    public async Task ItemTemplateEdit_UpdateEffects_ChangesPersist()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Create a new template with an effect
        var template = await dp.CreateAsync();
        template.Name = "Cursed Amulet";
        template.ItemType = ItemType.Jewelry;
        template.EquipmentSlot = EquipmentSlot.Neck;

        var effect = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect.Name = "Minor Curse";
        effect.Trigger = ItemEffectTrigger.WhilePossessed;

        // Save first version
        template = await template.SaveAsync();
        var savedId = template.Id;

        // Fetch, modify effect, and save again
        template = await dp.FetchAsync(savedId);
        template.Effects[0].Name = "Major Curse";
        template.Effects[0].IsCursed = true;
        template = await template.SaveAsync();

        // Fetch and verify changes
        var loaded = await dp.FetchAsync(savedId);

        // Assert
        Assert.AreEqual(1, loaded.Effects.Count, "Should still have 1 effect");
        Assert.AreEqual("Major Curse", loaded.Effects[0].Name, "Updated effect name should persist");
        Assert.IsTrue(loaded.Effects[0].IsCursed, "Updated IsCursed should persist");
    }

    [TestMethod]
    public async Task ItemTemplateEdit_AddMultipleEffects_AllPersist()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Create a new template with multiple effects
        var template = await dp.CreateAsync();
        template.Name = "Legendary Sword";
        template.ItemType = ItemType.Weapon;
        template.EquipmentSlot = EquipmentSlot.MainHand;

        var effect1 = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect1.Name = "Fire Damage";
        effect1.Trigger = ItemEffectTrigger.OnAttackWith;

        var effect2 = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect2.Name = "Strength Bonus";
        effect2.Trigger = ItemEffectTrigger.WhileEquipped;

        var effect3 = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect3.Name = "Curse of Bloodlust";
        effect3.Trigger = ItemEffectTrigger.OnPickup;
        effect3.IsCursed = true;

        // Act - Save
        template = await template.SaveAsync();
        var savedId = template.Id;

        // Fetch
        var loaded = await dp.FetchAsync(savedId);

        // Assert
        Assert.AreEqual(3, loaded.Effects.Count, "Should have 3 effects");
        Assert.IsTrue(loaded.Effects.Any(e => e.Name == "Fire Damage"), "Fire Damage effect should persist");
        Assert.IsTrue(loaded.Effects.Any(e => e.Name == "Strength Bonus"), "Strength Bonus effect should persist");
        Assert.IsTrue(loaded.Effects.Any(e => e.Name == "Curse of Bloodlust" && e.IsCursed),
            "Cursed effect should persist with IsCursed=true");
    }

    [TestMethod]
    public async Task ItemTemplateEdit_RemoveEffect_Persists()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<ItemEffectEdit>>();

        // Create template with two effects
        var template = await dp.CreateAsync();
        template.Name = "Test Item";
        template.ItemType = ItemType.Weapon;

        var effect1 = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect1.Name = "Effect 1";
        effect1.Trigger = ItemEffectTrigger.WhileEquipped;

        var effect2 = template.Effects.AddNewEffect(template.Id, effectPortal);
        effect2.Name = "Effect 2";
        effect2.Trigger = ItemEffectTrigger.OnUse;

        template = await template.SaveAsync();
        var savedId = template.Id;

        // Fetch, remove one effect, and save
        template = await dp.FetchAsync(savedId);
        Assert.AreEqual(2, template.Effects.Count, "Should have 2 effects before removal");

        var effectToRemove = template.Effects.First(e => e.Name == "Effect 1");
        template.Effects.Remove(effectToRemove);
        template = await template.SaveAsync();

        // Fetch and verify
        var loaded = await dp.FetchAsync(savedId);

        // Assert
        Assert.AreEqual(1, loaded.Effects.Count, "Should have 1 effect after removal");
        Assert.AreEqual("Effect 2", loaded.Effects[0].Name, "Remaining effect should be Effect 2");
    }

    #endregion
}
