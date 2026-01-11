using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for game tables (active sessions).
/// </summary>
public interface ITableDal
{
    /// <summary>
    /// Gets a blank table for creation.
    /// </summary>
    GameTable GetBlank();

    /// <summary>
    /// Gets all tables for a specific game master.
    /// </summary>
    Task<List<GameTable>> GetTablesByGmAsync(int gameMasterId);

    /// <summary>
    /// Gets all active tables (status != Ended).
    /// </summary>
    Task<List<GameTable>> GetActiveTablesAsync();

    /// <summary>
    /// Gets a specific table by ID.
    /// </summary>
    Task<GameTable> GetTableAsync(Guid id);

    /// <summary>
    /// Saves a table (insert or update).
    /// </summary>
    Task<GameTable> SaveTableAsync(GameTable table);

    /// <summary>
    /// Deletes a table and all associated data.
    /// </summary>
    Task DeleteTableAsync(Guid id);

    /// <summary>
    /// Adds a character to a table.
    /// </summary>
    Task<TableCharacter> AddCharacterToTableAsync(TableCharacter tableCharacter);

    /// <summary>
    /// Updates a character's status at a table.
    /// </summary>
    Task UpdateTableCharacterAsync(TableCharacter tableCharacter);

    /// <summary>
    /// Removes a character from a table.
    /// </summary>
    Task RemoveCharacterFromTableAsync(Guid tableId, int characterId);

    /// <summary>
    /// Gets all characters at a table.
    /// </summary>
    Task<List<TableCharacter>> GetTableCharactersAsync(Guid tableId);

    /// <summary>
    /// Adds an NPC to a table.
    /// </summary>
    Task<TableNpc> AddNpcToTableAsync(TableNpc npc);

    /// <summary>
    /// Updates an NPC at a table.
    /// </summary>
    Task UpdateTableNpcAsync(TableNpc npc);

    /// <summary>
    /// Removes an NPC from a table.
    /// </summary>
    Task RemoveNpcFromTableAsync(Guid npcId);

    /// <summary>
    /// Gets all NPCs at a table.
    /// </summary>
    Task<List<TableNpc>> GetTableNpcsAsync(Guid tableId);

    /// <summary>
    /// Gets tables that a specific character is connected to.
    /// </summary>
    Task<List<GameTable>> GetTablesForCharacterAsync(int characterId);
}
