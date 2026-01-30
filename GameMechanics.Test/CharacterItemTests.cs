using Csla;
using Csla.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Items;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class CharacterItemTests : TestBase
{

    #region CharacterItemEdit Tests

    [TestMethod]
    public void CharacterItemEdit_Create_InitializesWithCharacterAndTemplate()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();

        // Act
        var item = dp.Create(1, 10);

        // Assert
        Assert.AreEqual(1, item.OwnerCharacterId, "OwnerCharacterId should be set");
        Assert.AreEqual(10, item.ItemTemplateId, "ItemTemplateId should be set");
        Assert.AreEqual(1, item.StackSize, "StackSize should default to 1");
        Assert.IsFalse(item.IsEquipped, "IsEquipped should default to false");
        Assert.AreEqual(EquipmentSlot.None, item.EquippedSlot, "EquippedSlot should default to None");
        Assert.IsNull(item.ContainerItemId, "ContainerItemId should be null");
        Assert.IsTrue(string.IsNullOrEmpty(item.CustomName), "CustomName should be null or empty");
        Assert.IsNull(item.CurrentDurability, "CurrentDurability should be null");
    }

    [TestMethod]
    public void CharacterItemEdit_Create_GeneratesGuidId()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();

        // Act
        var item = dp.Create(1, 10);

        // Assert
        Assert.AreNotEqual(Guid.Empty, item.Id, "Id should not be empty GUID");
    }

    [TestMethod]
    public async Task CharacterItemEdit_Fetch_LoadsExistingItem()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();
        var expectedId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var item = await dp.FetchAsync(expectedId);

        // Assert
        Assert.AreEqual(expectedId, item.Id, "Id should match");
        Assert.AreEqual(10, item.ItemTemplateId, "ItemTemplateId should be Longsword (10)");
        Assert.AreEqual(1, item.OwnerCharacterId, "OwnerCharacterId should be 1");
        Assert.IsTrue(item.IsEquipped, "Longsword should be equipped");
        Assert.AreEqual(EquipmentSlot.MainHand, item.EquippedSlot, "Should be equipped in MainHand");
        Assert.AreEqual(100, item.CurrentDurability, "CurrentDurability should be 100");
    }

    [TestMethod]
    public async Task CharacterItemEdit_SaveAndFetch_PropertiesPersist()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();

        // Create a new item
        var item = dp.Create(1, 50); // Health Potion template
        item.StackSize = 5;
        item.CustomName = "Test Potion Stack";
        item.ContainerItemId = Guid.Parse("33333333-3333-3333-3333-333333333333"); // Backpack

        // Act - Save
        item = await item.SaveAsync();
        var savedId = item.Id;

        // Fetch the saved item
        var fetched = await dp.FetchAsync(savedId);

        // Assert
        Assert.AreEqual(savedId, fetched.Id, "Id should persist");
        Assert.AreEqual(1, fetched.OwnerCharacterId, "OwnerCharacterId should persist");
        Assert.AreEqual(50, fetched.ItemTemplateId, "ItemTemplateId should persist");
        Assert.AreEqual(5, fetched.StackSize, "StackSize should persist");
        Assert.AreEqual("Test Potion Stack", fetched.CustomName, "CustomName should persist");
        Assert.AreEqual(Guid.Parse("33333333-3333-3333-3333-333333333333"), fetched.ContainerItemId, "ContainerItemId should persist");
    }

    [TestMethod]
    public async Task CharacterItemEdit_Update_ChangesPersist()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();
        var itemId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Fetch existing item (Longsword)
        var item = await dp.FetchAsync(itemId);
        var originalName = item.CustomName;

        // Act - Update
        item.CustomName = "My Trusty Blade";
        item = await item.SaveAsync();

        // Fetch again
        var updated = await dp.FetchAsync(itemId);

        // Assert
        Assert.AreEqual("My Trusty Blade", updated.CustomName, "CustomName should be updated");

        // Cleanup - restore original state
        updated.CustomName = originalName;
        _ = await updated.SaveAsync();
    }

    [TestMethod]
    public void CharacterItemEdit_Validation_RequiresTemplateId()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();

        // Act - Create with invalid templateId
        var item = dp.Create(1, 0);

        // Assert
        Assert.IsFalse(item.IsSavable, "Item with ItemTemplateId=0 should not be savable");
        Assert.IsFalse(item.IsValid, "Item with ItemTemplateId=0 should not be valid");
    }

    [TestMethod]
    public void CharacterItemEdit_Validation_RequiresPositiveStackSize()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemEdit>>();

        // Act
        var item = dp.Create(1, 10);
        item.StackSize = 0;

        // Assert
        Assert.IsFalse(item.IsSavable, "Item with StackSize=0 should not be savable");
    }

    #endregion

    #region CharacterItemList Tests

    [TestMethod]
    public async Task CharacterItemList_Fetch_ReturnsCharacterItems()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemList>>();

        // Act
        var items = await dp.FetchAsync(1); // Character 1 has items in MockDb

        // Assert
        Assert.IsTrue(items.Count > 0, "Character 1 should have items");
    }

    [TestMethod]
    public async Task CharacterItemList_Fetch_ItemsHaveCorrectProperties()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemList>>();

        // Act
        var items = await dp.FetchAsync(1);

        // Find the Longsword (template 10)
        var longsword = items.FirstOrDefault(i => i.ItemTemplateId == 10);

        // Assert
        Assert.IsNotNull(longsword, "Character should have a Longsword");
        Assert.AreEqual(Guid.Parse("11111111-1111-1111-1111-111111111111"), longsword.Id, "Longsword should have expected Id");
        Assert.AreEqual(1, longsword.OwnerCharacterId, "Longsword should belong to character 1");
        Assert.IsTrue(longsword.IsEquipped, "Longsword should be equipped");
        Assert.AreEqual(EquipmentSlot.MainHand, longsword.EquippedSlot, "Longsword should be in MainHand");
    }

    [TestMethod]
    public async Task CharacterItemList_Fetch_ReturnsEmptyForUnknownCharacter()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterItemList>>();

        // Act
        var items = await dp.FetchAsync(9999); // Non-existent character

        // Assert
        Assert.AreEqual(0, items.Count, "Unknown character should have no items");
    }

    #endregion
}
