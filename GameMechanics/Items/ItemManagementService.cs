using Csla;
using GameMechanics.Combat;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Result of an item operation that may be blocked.
/// </summary>
public class ItemOperationResult
{
  /// <summary>
  /// Whether the operation succeeded.
  /// </summary>
  public bool Success { get; set; }

  /// <summary>
  /// Error message if the operation failed.
  /// </summary>
  public string? ErrorMessage { get; set; }

  /// <summary>
  /// The item that was operated on.
  /// </summary>
  public CharacterItem? Item { get; set; }

  public static ItemOperationResult Succeeded(CharacterItem? item = null) =>
    new() { Success = true, Item = item };

  public static ItemOperationResult Failed(string message) =>
    new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// High-level service for managing character items.
/// Coordinates between the DAL and the effect system to ensure
/// item effects are properly applied/removed during item operations.
/// </summary>
public class ItemManagementService
{
  private readonly ICharacterItemDal _itemDal;
  private readonly IItemTemplateDal _templateDal;
  private readonly Effects.ItemEffectService _effectService;
  private readonly AmmoCompatibilityValidator _ammoValidator;

  public ItemManagementService(
    ICharacterItemDal itemDal,
    IItemTemplateDal templateDal,
    Effects.ItemEffectService effectService)
  {
    _itemDal = itemDal;
    _templateDal = templateDal;
    _effectService = effectService;
    _ammoValidator = new AmmoCompatibilityValidator();
  }

  /// <summary>
  /// Adds a new item to a character's inventory and applies any possession-triggered effects.
  /// </summary>
  /// <param name="character">The character receiving the item.</param>
  /// <param name="item">The item to add.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> AddItemToInventoryAsync(
    CharacterEdit character,
    CharacterItem item)
  {
    try
    {
      // Get the template to check for effects
      var template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
      item.Template = template;

      // Add to DAL
      var addedItem = await _itemDal.AddItemAsync(item);

      // Apply possession effects (WhilePossessed, OnPickup)
      if (template.Effects.Count > 0)
      {
        var effectResult = await _effectService.OnItemPickedUpAsync(character, addedItem, template);
        if (!effectResult.Success)
        {
          // Effects failed but item was added - log warning but don't fail
          // The item is in inventory but effects may not be applied
        }
      }

      return ItemOperationResult.Succeeded(addedItem);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to add item: {ex.Message}");
    }
  }

  /// <summary>
  /// Equips an item from inventory to a specific slot.
  /// Applies equip-triggered effects and handles curse blocking.
  /// </summary>
  /// <param name="character">The character equipping the item.</param>
  /// <param name="itemId">The item to equip.</param>
  /// <param name="slot">The slot to equip to.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> EquipItemAsync(
    CharacterEdit character,
    Guid itemId,
    EquipmentSlot slot)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      // Check if item is in a container - must be in direct inventory to equip
      if (item.ContainerItemId.HasValue)
        return ItemOperationResult.Failed("Item must be removed from container before equipping.");

      // Get template for effects
      var template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);

      // Check if slot is compatible
      if (template.EquipmentSlot != EquipmentSlot.None && template.EquipmentSlot != slot)
      {
        // Allow some flexibility for weapon slots
        if (!IsCompatibleSlot(template.EquipmentSlot, slot))
          return ItemOperationResult.Failed($"This item cannot be equipped in the {slot.GetDisplayName()} slot.");
      }

      // Check for implant requirements
      if (slot.IsImplant())
      {
        // TODO: Check for surgery requirements
        // For now, just allow implant equipping
      }

      // Equip in DAL (handles unequipping existing items)
      await _itemDal.EquipItemAsync(itemId, slot);

      // Apply equip effects (WhileEquipped)
      if (template.Effects.Count > 0)
      {
        await _effectService.OnItemEquippedAsync(character, item, template);
      }

      item.IsEquipped = true;
      item.EquippedSlot = slot;
      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to equip item: {ex.Message}");
    }
  }

  /// <summary>
  /// Unequips an item, moving it to inventory.
  /// Checks for curse blocking and removes equip-triggered effects.
  /// </summary>
  /// <param name="character">The character unequipping the item.</param>
  /// <param name="itemId">The item to unequip.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> UnequipItemAsync(
    CharacterEdit character,
    Guid itemId)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      if (!item.IsEquipped)
        return ItemOperationResult.Failed("Item is not equipped.");

      // Check for curse blocking
      var curseCheck = _effectService.CanUnequipItem(character, itemId);
      if (!curseCheck.IsAllowed)
        return ItemOperationResult.Failed(curseCheck.BlockReason!);

      // Check for implant removal requirements
      if (item.EquippedSlot.IsImplant())
      {
        // TODO: Check for surgery requirements
        // For now, just allow implant removal
      }

      // Remove equip effects (but keep possession effects)
      _effectService.OnItemUnequipped(character, itemId);

      // Unequip in DAL
      await _itemDal.UnequipItemAsync(itemId);

      item.IsEquipped = false;
      item.EquippedSlot = EquipmentSlot.None;
      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to unequip item: {ex.Message}");
    }
  }

  /// <summary>
  /// Removes an item from a character's inventory (drop, sell, transfer).
  /// Checks for curse blocking and removes all item effects.
  /// </summary>
  /// <param name="character">The character losing the item.</param>
  /// <param name="itemId">The item to remove.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> RemoveItemFromInventoryAsync(
    CharacterEdit character,
    Guid itemId)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      // Check for curse blocking (both equip and possession curses)
      var curseCheck = _effectService.CanDropItem(character, itemId);
      if (!curseCheck.IsAllowed)
        return ItemOperationResult.Failed(curseCheck.BlockReason!);

      // If equipped, also check unequip curse
      if (item.IsEquipped)
      {
        var unequipCheck = _effectService.CanUnequipItem(character, itemId);
        if (!unequipCheck.IsAllowed)
          return ItemOperationResult.Failed(unequipCheck.BlockReason!);
      }

      // Remove all effects from this item
      _effectService.OnItemDropped(character, itemId);

      // Delete from DAL
      await _itemDal.DeleteItemAsync(itemId);

      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to remove item: {ex.Message}");
    }
  }

  /// <summary>
  /// Transfers an item from one character to another.
  /// </summary>
  /// <param name="fromCharacter">The character giving the item.</param>
  /// <param name="toCharacter">The character receiving the item.</param>
  /// <param name="itemId">The item to transfer.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> TransferItemAsync(
    CharacterEdit fromCharacter,
    CharacterEdit toCharacter,
    Guid itemId)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      // Check curse blocking on source character
      var curseCheck = _effectService.CanDropItem(fromCharacter, itemId);
      if (!curseCheck.IsAllowed)
        return ItemOperationResult.Failed(curseCheck.BlockReason!);

      if (item.IsEquipped)
      {
        var unequipCheck = _effectService.CanUnequipItem(fromCharacter, itemId);
        if (!unequipCheck.IsAllowed)
          return ItemOperationResult.Failed(unequipCheck.BlockReason!);
      }

      // Remove effects from source
      _effectService.OnItemDropped(fromCharacter, itemId);

      // Get template for applying effects to recipient
      var template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);

      // Update item ownership
      item.OwnerCharacterId = toCharacter.Id;
      item.IsEquipped = false;
      item.EquippedSlot = EquipmentSlot.None;
      item.ContainerItemId = null;
      await _itemDal.UpdateItemAsync(item);

      // Apply effects to recipient
      if (template.Effects.Count > 0)
      {
        await _effectService.OnItemPickedUpAsync(toCharacter, item, template);
      }

      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to transfer item: {ex.Message}");
    }
  }

  /// <summary>
  /// Moves an item into or out of a container.
  /// Validates ammo compatibility when moving ammo into an AmmoContainer.
  /// </summary>
  /// <param name="character">The character owning the items.</param>
  /// <param name="itemId">The item to move.</param>
  /// <param name="containerItemId">The container to move into (null for direct inventory).</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> MoveToContainerAsync(
    CharacterEdit character,
    Guid itemId,
    Guid? containerItemId)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      // If item is equipped, need to unequip first
      if (item.IsEquipped && containerItemId.HasValue)
      {
        var unequipResult = await UnequipItemAsync(character, itemId);
        if (!unequipResult.Success)
          return unequipResult;
      }

      // Validate ammo compatibility if moving into an AmmoContainer
      if (containerItemId.HasValue)
      {
        var containerItem = await _itemDal.GetItemAsync(containerItemId.Value);
        if (containerItem != null)
        {
          var containerTemplate = await _templateDal.GetTemplateAsync(containerItem.ItemTemplateId);

          // AmmoContainers (magazines, quivers, etc.) cannot receive items via drag-and-drop.
          // They must be loaded using the Reload button which properly tracks rounds and time.
          if (containerTemplate.ItemType == ItemType.AmmoContainer)
          {
            return ItemOperationResult.Failed(
              "Ammo containers must be loaded using the Reload button. " +
              "Select the magazine/quiver and click Reload.");
          }

          // Validate skill chip slot rules for implant containers
          if (containerTemplate.MaxChipSlots.HasValue)
          {
            // Only SkillChip items allowed in Skillwire implants
            var itemToMove = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
            if (itemToMove.ItemType != ItemType.SkillChip)
              return ItemOperationResult.Failed(
                "Only skill chips can be loaded into a Skillwire implant.");

            // Count current chips and check against slot limit
            var existingChips = await _itemDal.GetContainerContentsAsync(containerItemId.Value);
            if (existingChips.Count >= containerTemplate.MaxChipSlots.Value)
              return ItemOperationResult.Failed(
                $"This Skillwire is full ({containerTemplate.MaxChipSlots.Value} chip slots used).");
          }
        }
      }

      await _itemDal.MoveToContainerAsync(itemId, containerItemId);
      item.ContainerItemId = containerItemId;

      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to move item: {ex.Message}");
    }
  }

  /// <summary>
  /// Uses a consumable item, applying its effects.
  /// </summary>
  /// <param name="character">The character using the item.</param>
  /// <param name="itemId">The item to use.</param>
  /// <returns>Result of the operation.</returns>
  public async Task<ItemOperationResult> UseItemAsync(
    CharacterEdit character,
    Guid itemId)
  {
    try
    {
      var item = await _itemDal.GetItemAsync(itemId);
      if (item == null)
        return ItemOperationResult.Failed("Item not found.");

      var template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);

      // Check if item is usable
      if (template.ItemType != ItemType.Consumable)
        return ItemOperationResult.Failed("This item cannot be used.");

      // Apply OnUse effects
      foreach (var effectDef in template.Effects)
      {
        if (effectDef.IsActive && effectDef.Trigger == ItemEffectTrigger.OnUse)
        {
          // Create the effect - OnUse effects typically have a duration
          // and don't need to track the source item since the item is consumed
          // TODO: Create and apply the effect
        }
      }

      // Reduce stack or remove item
      if (item.StackSize > 1)
      {
        item.StackSize--;
        await _itemDal.UpdateItemAsync(item);
      }
      else
      {
        await _itemDal.DeleteItemAsync(itemId);
      }

      return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
      return ItemOperationResult.Failed($"Failed to use item: {ex.Message}");
    }
  }

  /// <summary>
  /// Checks if an item can be equipped (not blocked by curses).
  /// </summary>
  public bool CanEquipItem(CharacterEdit character, Guid itemId)
  {
    // Equipping is generally allowed unless there's some other restriction
    // The main restrictions are on unequipping
    return true;
  }

  /// <summary>
  /// Checks if an item can be unequipped.
  /// </summary>
  public bool CanUnequipItem(CharacterEdit character, Guid itemId)
  {
    return _effectService.CanUnequipItem(character, itemId).IsAllowed;
  }

  /// <summary>
  /// Checks if an item can be dropped or transferred.
  /// </summary>
  public bool CanDropItem(CharacterEdit character, Guid itemId)
  {
    return _effectService.CanDropItem(character, itemId).IsAllowed;
  }

  /// <summary>
  /// Gets the reason an item cannot be unequipped, if any.
  /// </summary>
  public string? GetUnequipBlockReason(CharacterEdit character, Guid itemId)
  {
    var check = _effectService.CanUnequipItem(character, itemId);
    return check.IsAllowed ? null : check.BlockReason;
  }

  /// <summary>
  /// Gets the reason an item cannot be dropped, if any.
  /// </summary>
  public string? GetDropBlockReason(CharacterEdit character, Guid itemId)
  {
    var check = _effectService.CanDropItem(character, itemId);
    return check.IsAllowed ? null : check.BlockReason;
  }

  private static bool IsCompatibleSlot(EquipmentSlot itemSlot, EquipmentSlot targetSlot)
  {
    // Weapons can go in MainHand, OffHand, or TwoHand based on weapon type
    if (itemSlot == EquipmentSlot.MainHand || itemSlot == EquipmentSlot.OffHand || itemSlot == EquipmentSlot.TwoHand)
    {
      return targetSlot == EquipmentSlot.MainHand || 
             targetSlot == EquipmentSlot.OffHand || 
             targetSlot == EquipmentSlot.TwoHand;
    }

    // Rings can go in any finger slot
    if (itemSlot.IsFingerSlot())
    {
      return targetSlot.IsFingerSlot();
    }

    // Otherwise, must match exactly
    return itemSlot == targetSlot;
  }
}
