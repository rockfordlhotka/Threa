using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of ICharacterItemDal for development and testing.
/// </summary>
public class CharacterItemDal : ICharacterItemDal
{
    public Task<List<CharacterItem>> GetCharacterItemsAsync(int characterId)
    {
        var items = MockDb.CharacterItems
            .Where(i => i.OwnerCharacterId == characterId)
            .Select(i => PopulateTemplate(i))
            .ToList();
        return Task.FromResult(items);
    }

    public Task<List<CharacterItem>> GetEquippedItemsAsync(int characterId)
    {
        var items = MockDb.CharacterItems
            .Where(i => i.OwnerCharacterId == characterId && i.IsEquipped)
            .Select(i => PopulateTemplate(i))
            .ToList();
        return Task.FromResult(items);
    }

    public Task<List<CharacterItem>> GetContainerContentsAsync(Guid containerItemId)
    {
        var items = MockDb.CharacterItems
            .Where(i => i.ContainerItemId == containerItemId)
            .Select(i => PopulateTemplate(i))
            .ToList();
        return Task.FromResult(items);
    }

    public Task<CharacterItem> GetItemAsync(Guid id)
    {
        var item = MockDb.CharacterItems.FirstOrDefault(i => i.Id == id);
        if (item == null)
            throw new NotFoundException($"CharacterItem {id}");
        return Task.FromResult(PopulateTemplate(item));
    }

    public Task<CharacterItem> AddItemAsync(CharacterItem item)
    {
        if (item.Id == Guid.Empty)
            item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;
        MockDb.CharacterItems.Add(item);
        return Task.FromResult(PopulateTemplate(item));
    }

    public Task<CharacterItem> UpdateItemAsync(CharacterItem item)
    {
        var existing = MockDb.CharacterItems.FirstOrDefault(i => i.Id == item.Id);
        if (existing == null)
            throw new NotFoundException($"CharacterItem {item.Id}");
        MockDb.CharacterItems.Remove(existing);
        MockDb.CharacterItems.Add(item);
        return Task.FromResult(PopulateTemplate(item));
    }

    public Task DeleteItemAsync(Guid id)
    {
        var item = MockDb.CharacterItems.FirstOrDefault(i => i.Id == id);
        if (item == null)
            throw new NotFoundException($"CharacterItem {id}");
        MockDb.CharacterItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task EquipItemAsync(Guid itemId, EquipmentSlot slot)
    {
        var item = MockDb.CharacterItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException($"CharacterItem {itemId}");
        
        // Check if item is in a container - can't equip from inside containers
        if (item.ContainerItemId.HasValue)
            throw new OperationFailedException("Cannot equip items directly from inside containers. Move to inventory first.");

        // Handle two-handed weapon equipping
        if (slot == EquipmentSlot.TwoHand)
        {
            // Unequip MainHand and OffHand if occupied
            UnequipSlot(item.OwnerCharacterId, EquipmentSlot.MainHand);
            UnequipSlot(item.OwnerCharacterId, EquipmentSlot.OffHand);
        }
        else if (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand)
        {
            // Unequip any two-handed weapon
            UnequipSlot(item.OwnerCharacterId, EquipmentSlot.TwoHand);
        }

        // Unequip any item currently in the target slot
        UnequipSlot(item.OwnerCharacterId, slot);

        item.IsEquipped = true;
        item.EquippedSlot = slot;
        return Task.CompletedTask;
    }

    public Task UnequipItemAsync(Guid itemId)
    {
        var item = MockDb.CharacterItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException($"CharacterItem {itemId}");
        
        item.IsEquipped = false;
        item.EquippedSlot = EquipmentSlot.None;
        return Task.CompletedTask;
    }

    public Task MoveToContainerAsync(Guid itemId, Guid? containerItemId)
    {
        var item = MockDb.CharacterItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException($"CharacterItem {itemId}");
        
        // If equipping, must unequip first
        if (item.IsEquipped && containerItemId.HasValue)
        {
            item.IsEquipped = false;
            item.EquippedSlot = EquipmentSlot.None;
        }

        // Validate container exists and is a container
        if (containerItemId.HasValue)
        {
            var container = MockDb.CharacterItems.FirstOrDefault(i => i.Id == containerItemId.Value);
            if (container == null)
                throw new NotFoundException($"Container {containerItemId}");
            
            var template = MockDb.ItemTemplates.FirstOrDefault(t => t.Id == container.ItemTemplateId);
            if (template == null || !template.IsContainer)
                throw new OperationFailedException("Target item is not a container");
        }

        item.ContainerItemId = containerItemId;
        return Task.CompletedTask;
    }

    private void UnequipSlot(int characterId, EquipmentSlot slot)
    {
        var equippedItem = MockDb.CharacterItems
            .FirstOrDefault(i => i.OwnerCharacterId == characterId && 
                                  i.IsEquipped && 
                                  i.EquippedSlot == slot);
        if (equippedItem != null)
        {
            equippedItem.IsEquipped = false;
            equippedItem.EquippedSlot = EquipmentSlot.None;
        }
    }

    private CharacterItem PopulateTemplate(CharacterItem item)
    {
        item.Template = MockDb.ItemTemplates.FirstOrDefault(t => t.Id == item.ItemTemplateId);
        return item;
    }
}
