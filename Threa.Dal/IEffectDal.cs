using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for effect definitions (templates).
/// </summary>
public interface IEffectDefinitionDal
{
    /// <summary>
    /// Gets all active effect definitions.
    /// </summary>
    Task<List<EffectDefinition>> GetAllDefinitionsAsync();

    /// <summary>
    /// Gets effect definitions filtered by type.
    /// </summary>
    /// <param name="effectType">The type of effects to retrieve.</param>
    Task<List<EffectDefinition>> GetDefinitionsByTypeAsync(EffectType effectType);

    /// <summary>
    /// Gets a specific effect definition by ID.
    /// </summary>
    /// <param name="id">The definition ID.</param>
    Task<EffectDefinition> GetDefinitionAsync(int id);

    /// <summary>
    /// Gets an effect definition by name.
    /// </summary>
    /// <param name="name">The effect name.</param>
    Task<EffectDefinition?> GetDefinitionByNameAsync(string name);

    /// <summary>
    /// Searches for effect definitions by name.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    Task<List<EffectDefinition>> SearchDefinitionsAsync(string searchTerm);

    /// <summary>
    /// Creates or updates an effect definition.
    /// </summary>
    /// <param name="definition">The definition to save.</param>
    Task<EffectDefinition> SaveDefinitionAsync(EffectDefinition definition);

    /// <summary>
    /// Deactivates an effect definition (soft delete).
    /// </summary>
    /// <param name="id">The definition ID to deactivate.</param>
    Task DeactivateDefinitionAsync(int id);
}

/// <summary>
/// Data access layer for character effect instances.
/// </summary>
public interface ICharacterEffectDal
{
    /// <summary>
    /// Gets all active effects on a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<CharacterEffect>> GetCharacterEffectsAsync(int characterId);

    /// <summary>
    /// Gets effects on a character filtered by type.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="effectType">The type of effects to retrieve.</param>
    Task<List<CharacterEffect>> GetCharacterEffectsByTypeAsync(int characterId, EffectType effectType);

    /// <summary>
    /// Gets a specific effect instance by ID.
    /// </summary>
    /// <param name="id">The effect instance ID.</param>
    Task<CharacterEffect?> GetEffectAsync(Guid id);

    /// <summary>
    /// Checks if a character has a specific effect (by definition name).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="effectName">The effect definition name.</param>
    Task<bool> HasEffectAsync(int characterId, string effectName);

    /// <summary>
    /// Gets effects by definition name.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="effectName">The effect definition name.</param>
    Task<List<CharacterEffect>> GetEffectsByNameAsync(int characterId, string effectName);

    /// <summary>
    /// Applies a new effect to a character.
    /// Handles stacking behavior according to the effect definition.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    Task<CharacterEffect> ApplyEffectAsync(CharacterEffect effect);

    /// <summary>
    /// Updates an existing effect instance.
    /// </summary>
    /// <param name="effect">The effect to update.</param>
    Task<CharacterEffect> UpdateEffectAsync(CharacterEffect effect);

    /// <summary>
    /// Removes an effect from a character.
    /// </summary>
    /// <param name="id">The effect instance ID.</param>
    Task RemoveEffectAsync(Guid id);

    /// <summary>
    /// Removes all effects of a specific type from a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="effectType">The type of effects to remove.</param>
    Task RemoveEffectsByTypeAsync(int characterId, EffectType effectType);

    /// <summary>
    /// Removes expired effects (where EndTime has passed).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task RemoveExpiredEffectsAsync(int characterId);

    /// <summary>
    /// Decrements round-based effect durations and removes expired ones.
    /// Called at end of each combat round.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of effects that expired this round.</returns>
    Task<List<CharacterEffect>> ProcessEndOfRoundAsync(int characterId);
}

/// <summary>
/// Data access layer for item effect instances.
/// </summary>
public interface IItemEffectDal
{
    /// <summary>
    /// Gets all active effects on an item.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    Task<List<ItemEffect>> GetItemEffectsAsync(Guid itemId);

    /// <summary>
    /// Gets all effects on items owned by a character.
    /// This includes effects on both equipped and inventory items.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<ItemEffect>> GetAllItemEffectsForCharacterAsync(int characterId);

    /// <summary>
    /// Gets effects on items equipped by a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task<List<ItemEffect>> GetEquippedItemEffectsAsync(int characterId);

    /// <summary>
    /// Gets a specific item effect instance by ID.
    /// </summary>
    /// <param name="id">The effect instance ID.</param>
    Task<ItemEffect?> GetEffectAsync(Guid id);

    /// <summary>
    /// Applies a new effect to an item.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    Task<ItemEffect> ApplyEffectAsync(ItemEffect effect);

    /// <summary>
    /// Updates an existing item effect instance.
    /// </summary>
    /// <param name="effect">The effect to update.</param>
    Task<ItemEffect> UpdateEffectAsync(ItemEffect effect);

    /// <summary>
    /// Removes an effect from an item.
    /// </summary>
    /// <param name="id">The effect instance ID.</param>
    Task RemoveEffectAsync(Guid id);

    /// <summary>
    /// Removes all effects from an item.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    Task RemoveAllEffectsAsync(Guid itemId);

    /// <summary>
    /// Removes expired effects on items owned by a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task RemoveExpiredEffectsAsync(int characterId);

    /// <summary>
    /// Processes end of round for item effects.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of item effects that expired this round.</returns>
    Task<List<ItemEffect>> ProcessEndOfRoundAsync(int characterId);
}
