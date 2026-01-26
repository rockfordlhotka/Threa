using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for item templates (world item definitions).
/// </summary>
public interface IItemTemplateDal
{
    /// <summary>
    /// Gets all active item templates.
    /// </summary>
    Task<List<ItemTemplate>> GetAllTemplatesAsync();

    /// <summary>
    /// Gets item templates filtered by type.
    /// </summary>
    /// <param name="itemType">The type of items to retrieve.</param>
    Task<List<ItemTemplate>> GetTemplatesByTypeAsync(ItemType itemType);

    /// <summary>
    /// Gets a specific item template by ID.
    /// </summary>
    /// <param name="id">The template ID.</param>
    Task<ItemTemplate> GetTemplateAsync(int id);

    /// <summary>
    /// Searches for item templates by name.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    Task<List<ItemTemplate>> SearchTemplatesAsync(string searchTerm);

    /// <summary>
    /// Creates or updates an item template.
    /// </summary>
    /// <param name="template">The template to save.</param>
    Task<ItemTemplate> SaveTemplateAsync(ItemTemplate template);

    /// <summary>
    /// Deactivates an item template (soft delete).
    /// </summary>
    /// <param name="id">The template ID to deactivate.</param>
    Task DeactivateTemplateAsync(int id);
}

/// <summary>
/// Data access layer for character item instances.
/// </summary>
public interface ICharacterItemDal
{
    /// <summary>
    /// Gets all items owned by a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<CharacterItem>> GetCharacterItemsAsync(int characterId);

    /// <summary>
    /// Gets all equipped items for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<CharacterItem>> GetEquippedItemsAsync(int characterId);

    /// <summary>
    /// Gets items inside a specific container.
    /// </summary>
    /// <param name="containerItemId">The container item ID.</param>
    Task<List<CharacterItem>> GetContainerContentsAsync(Guid containerItemId);

    /// <summary>
    /// Gets a specific item instance by ID.
    /// </summary>
    /// <param name="id">The item instance ID.</param>
    Task<CharacterItem> GetItemAsync(Guid id);

    /// <summary>
    /// Adds a new item to a character's inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    Task<CharacterItem> AddItemAsync(CharacterItem item);

    /// <summary>
    /// Updates an existing item instance.
    /// </summary>
    /// <param name="item">The item to update.</param>
    Task<CharacterItem> UpdateItemAsync(CharacterItem item);

    /// <summary>
    /// Removes an item from a character's inventory.
    /// </summary>
    /// <param name="id">The item instance ID.</param>
    Task DeleteItemAsync(Guid id);

    /// <summary>
    /// Equips an item to a specific slot.
    /// </summary>
    /// <param name="itemId">The item to equip.</param>
    /// <param name="slot">The slot to equip to.</param>
    Task EquipItemAsync(Guid itemId, EquipmentSlot slot);

    /// <summary>
    /// Unequips an item.
    /// </summary>
    /// <param name="itemId">The item to unequip.</param>
    Task UnequipItemAsync(Guid itemId);

    /// <summary>
    /// Moves an item into a container.
    /// </summary>
    /// <param name="itemId">The item to move.</param>
    /// <param name="containerItemId">The container to move it into (null for direct inventory).</param>
    Task MoveToContainerAsync(Guid itemId, Guid? containerItemId);

    /// <summary>
    /// Gets equipped items for a character with their templates populated.
    /// Used for bonus calculations where template data is required.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<CharacterItem>> GetEquippedItemsWithTemplatesAsync(int characterId);
}
