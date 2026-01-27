using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public class TableDal : ITableDal
{
    public GameTable GetBlank()
    {
        return new GameTable
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = TableStatus.Lobby
        };
    }

    public Task<List<GameTable>> GetTablesByGmAsync(int gameMasterId)
    {
        var tables = MockDb.Tables.Where(t => t.GameMasterId == gameMasterId).ToList();
        return Task.FromResult(tables);
    }

    public Task<List<GameTable>> GetActiveTablesAsync()
    {
        var tables = MockDb.Tables.Where(t => t.Status != TableStatus.Ended).ToList();
        return Task.FromResult(tables);
    }

    public Task<GameTable> GetTableAsync(Guid id)
    {
        var table = MockDb.Tables.FirstOrDefault(t => t.Id == id);
        if (table == null)
            throw new NotFoundException($"GameTable {id}");
        return Task.FromResult(table);
    }

    public Task<GameTable> SaveTableAsync(GameTable table)
    {
        lock (MockDb.Tables)
        {
            table.LastActivityAt = DateTime.UtcNow;

            var existing = MockDb.Tables.FirstOrDefault(t => t.Id == table.Id);
            if (existing == null)
            {
                MockDb.Tables.Add(table);
            }
            else if (!ReferenceEquals(existing, table))
            {
                MockDb.Tables.Remove(existing);
                MockDb.Tables.Add(table);
            }
        }
        return Task.FromResult(table);
    }

    public Task DeleteTableAsync(Guid id)
    {
        var table = MockDb.Tables.FirstOrDefault(t => t.Id == id);
        if (table != null)
        {
            MockDb.Tables.Remove(table);
            MockDb.TableCharacters.RemoveAll(tc => tc.TableId == id);
            MockDb.TableNpcs.RemoveAll(n => n.TableId == id);
        }
        return Task.CompletedTask;
    }

    public Task<TableCharacter> AddCharacterToTableAsync(TableCharacter tableCharacter)
    {
        MockDb.TableCharacters.Add(tableCharacter);
        return Task.FromResult(tableCharacter);
    }

    public Task UpdateTableCharacterAsync(TableCharacter tableCharacter)
    {
        var existing = MockDb.TableCharacters.FirstOrDefault(
            tc => tc.TableId == tableCharacter.TableId && tc.CharacterId == tableCharacter.CharacterId);
        if (existing != null && !ReferenceEquals(existing, tableCharacter))
        {
            MockDb.TableCharacters.Remove(existing);
            MockDb.TableCharacters.Add(tableCharacter);
        }
        return Task.CompletedTask;
    }

    public Task RemoveCharacterFromTableAsync(Guid tableId, int characterId)
    {
        MockDb.TableCharacters.RemoveAll(tc => tc.TableId == tableId && tc.CharacterId == characterId);
        return Task.CompletedTask;
    }

    public Task<List<TableCharacter>> GetTableCharactersAsync(Guid tableId)
    {
        var characters = MockDb.TableCharacters.Where(tc => tc.TableId == tableId).ToList();
        return Task.FromResult(characters);
    }

    public Task<TableNpc> AddNpcToTableAsync(TableNpc npc)
    {
        if (npc.Id == Guid.Empty)
            npc.Id = Guid.NewGuid();
        MockDb.TableNpcs.Add(npc);
        return Task.FromResult(npc);
    }

    public Task UpdateTableNpcAsync(TableNpc npc)
    {
        var existing = MockDb.TableNpcs.FirstOrDefault(n => n.Id == npc.Id);
        if (existing != null && !ReferenceEquals(existing, npc))
        {
            MockDb.TableNpcs.Remove(existing);
            MockDb.TableNpcs.Add(npc);
        }
        return Task.CompletedTask;
    }

    public Task RemoveNpcFromTableAsync(Guid npcId)
    {
        MockDb.TableNpcs.RemoveAll(n => n.Id == npcId);
        return Task.CompletedTask;
    }

    public Task<List<TableNpc>> GetTableNpcsAsync(Guid tableId)
    {
        var npcs = MockDb.TableNpcs.Where(n => n.TableId == tableId).ToList();
        return Task.FromResult(npcs);
    }

    public Task<List<GameTable>> GetTablesForCharacterAsync(int characterId)
    {
        var tableIds = MockDb.TableCharacters
            .Where(tc => tc.CharacterId == characterId)
            .Select(tc => tc.TableId)
            .ToHashSet();
        var tables = MockDb.Tables.Where(t => tableIds.Contains(t.Id)).ToList();
        return Task.FromResult(tables);
    }

    public Task<GameTable?> GetTableForCharacterAsync(int characterId)
    {
        var tableCharacter = MockDb.TableCharacters
            .FirstOrDefault(tc => tc.CharacterId == characterId);
        if (tableCharacter == null)
            return Task.FromResult<GameTable?>(null);

        var table = MockDb.Tables.FirstOrDefault(t => t.Id == tableCharacter.TableId);
        return Task.FromResult(table);
    }

    public Task UpdateGmNotesAsync(Guid tableId, int characterId, string? notes)
    {
        var tableCharacter = MockDb.TableCharacters
            .FirstOrDefault(tc => tc.TableId == tableId && tc.CharacterId == characterId);
        if (tableCharacter != null)
        {
            tableCharacter.GmNotes = notes;
        }
        return Task.CompletedTask;
    }
}
