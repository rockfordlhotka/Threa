using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for character mana pools.
/// </summary>
public interface IManaDal
{
    /// <summary>
    /// Gets a character's mana pool for a specific magic school.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school.</param>
    /// <returns>The mana pool, or null if character has no mana in that school.</returns>
    Task<CharacterMana?> GetManaPoolAsync(int characterId, MagicSchool school);

    /// <summary>
    /// Gets all mana pools for a character.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <returns>List of mana pools for all schools the character has.</returns>
    Task<List<CharacterMana>> GetAllManaPoolsAsync(int characterId);

    /// <summary>
    /// Creates or updates a character's mana pool for a school.
    /// </summary>
    /// <param name="manaPool">The mana pool to save.</param>
    /// <returns>The saved mana pool with updated ID.</returns>
    Task<CharacterMana> SaveManaPoolAsync(CharacterMana manaPool);

    /// <summary>
    /// Updates just the current mana value for a pool.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school.</param>
    /// <param name="currentMana">The new current mana value.</param>
    Task UpdateCurrentManaAsync(int characterId, MagicSchool school, int currentMana);

    /// <summary>
    /// Initializes mana pools for a character based on their mana skills.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    Task InitializeManaPoolsAsync(int characterId);
}
