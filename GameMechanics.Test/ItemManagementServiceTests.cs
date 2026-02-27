using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using GameMechanics.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ItemManagementServiceTests : TestBase
{
  protected override void ConfigureAdditionalServices(IServiceCollection services)
  {
    services.AddGameMechanics();
  }

  // Use existing template IDs from TestDataSeeder (Id=10 = Longsword with MainHand slot)
  private const int SWORD_TEMPLATE_ID = 10;

  #region AddItemToInventoryAsync Tests

  [TestMethod]
  public async Task AddItemToInventoryAsync_AddsItemSuccessfully()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };

    var result = await service.AddItemToInventoryAsync(character, item);

    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Item);
  }

  #endregion

  #region EquipItemAsync Tests

  [TestMethod]
  public async Task EquipItemAsync_EquipsItemSuccessfully()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID, // Longsword: EquipmentSlot=MainHand → EquipmentSlots=[MainHand]
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(item);

    // Single-slot item: chosenSlot is accepted but template slot is used
    var result = await service.EquipItemAsync(character, item.Id, EquipmentSlot.MainHand);

    Assert.IsTrue(result.Success, $"Equip failed: {result.ErrorMessage}");
    Assert.IsTrue(result.Item?.IsEquipped);
    Assert.AreEqual(EquipmentSlot.MainHand, result.Item?.EquippedSlot);
    Assert.AreEqual(1, result.Item?.EquippedSlots.Count);
    Assert.AreEqual(EquipmentSlot.MainHand, result.Item?.EquippedSlots[0]);
  }

  [TestMethod]
  public async Task EquipItemAsync_MultiSlotItem_OccupiesAllSlots()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var templateDal = provider.GetRequiredService<IItemTemplateDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Create a Boots template that occupies both feet simultaneously
    var bootsTemplate = await templateDal.SaveTemplateAsync(new ItemTemplate
    {
      Name = "Iron Boots",
      ItemType = ItemType.Armor,
      EquipmentSlots = [EquipmentSlot.FootLeft, EquipmentSlot.FootRight],
      OccupiesAllSlots = true,
      IsActive = true,
      Weight = 3m
    });

    var boots = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = bootsTemplate.Id,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(boots);

    var result = await service.EquipItemAsync(character, boots.Id);

    Assert.IsTrue(result.Success, $"Equip failed: {result.ErrorMessage}");
    Assert.IsTrue(result.Item?.IsEquipped);
    Assert.AreEqual(2, result.Item?.EquippedSlots.Count);
    Assert.IsTrue(result.Item?.EquippedSlots.Contains(EquipmentSlot.FootLeft));
    Assert.IsTrue(result.Item?.EquippedSlots.Contains(EquipmentSlot.FootRight));
  }

  [TestMethod]
  public async Task EquipItemAsync_MultiSlotItem_UnequipsConflictingItems()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var templateDal = provider.GetRequiredService<IItemTemplateDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Create a Boots template (occupies both feet)
    var bootsTemplate = await templateDal.SaveTemplateAsync(new ItemTemplate
    {
      Name = "Iron Boots",
      ItemType = ItemType.Armor,
      EquipmentSlots = [EquipmentSlot.FootLeft, EquipmentSlot.FootRight],
      OccupiesAllSlots = true,
      IsActive = true,
      Weight = 3m
    });
    // Create a Sandal template (one foot)
    var sandalTemplate = await templateDal.SaveTemplateAsync(new ItemTemplate
    {
      Name = "Left Sandal",
      ItemType = ItemType.Armor,
      EquipmentSlots = [EquipmentSlot.FootLeft],
      OccupiesAllSlots = false,
      IsActive = true,
      Weight = 0.5m
    });

    var sandal = new CharacterItem { Id = Guid.NewGuid(), ItemTemplateId = sandalTemplate.Id, OwnerCharacterId = character.Id, StackSize = 1 };
    var boots = new CharacterItem { Id = Guid.NewGuid(), ItemTemplateId = bootsTemplate.Id, OwnerCharacterId = character.Id, StackSize = 1 };
    await itemDal.AddItemAsync(sandal);
    await itemDal.AddItemAsync(boots);

    // Equip sandal to FootLeft
    await service.EquipItemAsync(character, sandal.Id);

    // Now equip boots — should unequip the sandal
    var result = await service.EquipItemAsync(character, boots.Id);

    Assert.IsTrue(result.Success, $"Equip failed: {result.ErrorMessage}");
    Assert.IsTrue(result.Item?.IsEquipped);
    Assert.AreEqual(2, result.Item?.EquippedSlots.Count);

    // Verify sandal was unequipped
    var sandalReloaded = await itemDal.GetItemAsync(sandal.Id);
    Assert.IsFalse(sandalReloaded.IsEquipped, "Sandal should have been unequipped by the boots");
    Assert.AreEqual(0, sandalReloaded.EquippedSlots.Count);
  }

  [TestMethod]
  public async Task EquipItemAsync_ChooseableSlotItem_UsesChosenSlot()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var templateDal = provider.GetRequiredService<IItemTemplateDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Create a Ring template with all left-hand finger slots (player picks one)
    var ringTemplate = await templateDal.SaveTemplateAsync(new ItemTemplate
    {
      Name = "Ring of Power",
      ItemType = ItemType.Jewelry,
      EquipmentSlots = [
        EquipmentSlot.FingerLeft1, EquipmentSlot.FingerLeft2, EquipmentSlot.FingerLeft3,
        EquipmentSlot.FingerLeft4, EquipmentSlot.FingerLeft5,
        EquipmentSlot.FingerRight1, EquipmentSlot.FingerRight2, EquipmentSlot.FingerRight3,
        EquipmentSlot.FingerRight4, EquipmentSlot.FingerRight5
      ],
      OccupiesAllSlots = false,
      IsActive = true,
      Weight = 0.05m
    });

    var ring = new CharacterItem { Id = Guid.NewGuid(), ItemTemplateId = ringTemplate.Id, OwnerCharacterId = character.Id, StackSize = 1 };
    await itemDal.AddItemAsync(ring);

    // Equip ring to a specific finger
    var result = await service.EquipItemAsync(character, ring.Id, EquipmentSlot.FingerLeft3);

    Assert.IsTrue(result.Success, $"Equip failed: {result.ErrorMessage}");
    Assert.IsTrue(result.Item?.IsEquipped);
    Assert.AreEqual(EquipmentSlot.FingerLeft3, result.Item?.EquippedSlot);
    Assert.AreEqual(1, result.Item?.EquippedSlots.Count);
    Assert.AreEqual(EquipmentSlot.FingerLeft3, result.Item?.EquippedSlots[0]);
  }

  #endregion

  #region UnequipItemAsync Tests

  [TestMethod]
  public async Task UnequipItemAsync_UnequipsSuccessfully()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.MainHand
    };
    await itemDal.AddItemAsync(item);

    var result = await service.UnequipItemAsync(character, item.Id);

    Assert.IsTrue(result.Success);
    Assert.IsFalse(result.Item?.IsEquipped);
    Assert.AreEqual(EquipmentSlot.None, result.Item?.EquippedSlot);
  }

  [TestMethod]
  public async Task UnequipItemAsync_RemovesEquipEffects()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.MainHand
    };
    await itemDal.AddItemAsync(item);

    // Manually add an equip effect
    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Strength Boost",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    character.Effects.AddEffect(effectPortal.CreateChild(equipDef, item.Id));
    Assert.AreEqual(1, character.Effects.Count);

    // Then unequip
    var result = await service.UnequipItemAsync(character, item.Id);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(0, character.Effects.Count);
  }

  [TestMethod]
  public async Task UnequipItemAsync_KeepsPossessionEffects()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.MainHand
    };
    await itemDal.AddItemAsync(item);

    // Manually add both equip and possession effects
    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Effect",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Effect",
      Trigger = ItemEffectTrigger.WhilePossessed
    };
    character.Effects.AddEffect(effectPortal.CreateChild(equipDef, item.Id));
    character.Effects.AddEffect(effectPortal.CreateChild(possessDef, item.Id));

    Assert.AreEqual(2, character.Effects.Count);

    var result = await service.UnequipItemAsync(character, item.Id);

    Assert.IsTrue(result.Success, $"Unequip failed: {result.ErrorMessage}");
    Assert.AreEqual(1, character.Effects.Count);
    Assert.AreEqual("Possess Effect", character.Effects[0].Name);
  }

  [TestMethod]
  public async Task UnequipItemAsync_BlockedByCurse()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.MainHand
    };
    await itemDal.AddItemAsync(item);

    // Add a cursed effect
    var curseDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Dark Binding",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    character.Effects.AddEffect(effectPortal.CreateChild(curseDef, item.Id));

    var result = await service.UnequipItemAsync(character, item.Id);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage?.Contains("Dark Binding"));
    Assert.AreEqual(1, character.Effects.Count); // Curse still there
  }

  #endregion

  #region RemoveItemFromInventoryAsync Tests

  [TestMethod]
  public async Task RemoveItemFromInventoryAsync_RemovesSuccessfully()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(item);

    var result = await service.RemoveItemFromInventoryAsync(character, item.Id);

    Assert.IsTrue(result.Success);
  }

  [TestMethod]
  public async Task RemoveItemFromInventoryAsync_RemovesAllEffects()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(item);

    // Add effects
    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Effect",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Effect",
      Trigger = ItemEffectTrigger.WhilePossessed
    };
    character.Effects.AddEffect(effectPortal.CreateChild(equipDef, item.Id));
    character.Effects.AddEffect(effectPortal.CreateChild(possessDef, item.Id));

    Assert.AreEqual(2, character.Effects.Count);

    var result = await service.RemoveItemFromInventoryAsync(character, item.Id);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(0, character.Effects.Count);
  }

  [TestMethod]
  public async Task RemoveItemFromInventoryAsync_BlockedByPossessionCurse()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var item = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SWORD_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(item);

    // Add a cursed possession effect
    var curseDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Soul Bound",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };
    character.Effects.AddEffect(effectPortal.CreateChild(curseDef, item.Id));

    var result = await service.RemoveItemFromInventoryAsync(character, item.Id);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage?.Contains("Soul Bound"));
  }

  #endregion

  #region CanUnequipItem / CanDropItem Tests

  [TestMethod]
  public void CanUnequipItem_ReturnsTrueWhenNotCursed()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var itemId = Guid.NewGuid();
    Assert.IsTrue(service.CanUnequipItem(character, itemId));
  }

  [TestMethod]
  public void CanUnequipItem_ReturnsFalseWhenCursed()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var itemId = Guid.NewGuid();
    var curseDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    character.Effects.AddEffect(effectPortal.CreateChild(curseDef, itemId));

    Assert.IsFalse(service.CanUnequipItem(character, itemId));
  }

  [TestMethod]
  public void CanDropItem_ReturnsFalseWhenPossessionCursed()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var itemId = Guid.NewGuid();
    var curseDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Bound",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };
    character.Effects.AddEffect(effectPortal.CreateChild(curseDef, itemId));

    Assert.IsFalse(service.CanDropItem(character, itemId));
  }

  [TestMethod]
  public void GetUnequipBlockReason_ReturnsReasonWhenCursed()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var itemId = Guid.NewGuid();
    var curseDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Evil Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    character.Effects.AddEffect(effectPortal.CreateChild(curseDef, itemId));

    var reason = service.GetUnequipBlockReason(character, itemId);

    Assert.IsNotNull(reason);
    Assert.IsTrue(reason.Contains("Evil Curse"));
  }

  #endregion
}
