using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of IManaDal.
/// </summary>
public class ManaDal : IManaDal
{
    private readonly SqliteConnection Connection;

    public ManaDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS CharacterMana (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    CharacterId INTEGER NOT NULL,
                    MagicSchool INTEGER NOT NULL,
                    Json TEXT NOT NULL,
                    UNIQUE(CharacterId, MagicSchool)
                );
                CREATE INDEX IF NOT EXISTS IX_CharacterMana_CharacterId ON CharacterMana(CharacterId);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating CharacterMana table", ex);
        }
    }

    public async Task<CharacterMana?> GetManaPoolAsync(int characterId, MagicSchool school)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterMana WHERE CharacterId = @CharacterId AND MagicSchool = @MagicSchool";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@MagicSchool", (int)school);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<CharacterMana>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting mana pool for character {characterId}, school {school}", ex);
        }
    }

    public async Task<List<CharacterMana>> GetAllManaPoolsAsync(int characterId)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterMana WHERE CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();
            var pools = new List<CharacterMana>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var pool = JsonSerializer.Deserialize<CharacterMana>(json);
                if (pool != null)
                    pools.Add(pool);
            }
            return pools;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting mana pools for character {characterId}", ex);
        }
    }

    public async Task<CharacterMana> SaveManaPoolAsync(CharacterMana manaPool)
    {
        try
        {
            // Check if exists
            var existing = await GetManaPoolAsync(manaPool.CharacterId, manaPool.MagicSchool);
            string sql;

            if (existing == null)
            {
                sql = @"INSERT INTO CharacterMana (CharacterId, MagicSchool, Json)
                        VALUES (@CharacterId, @MagicSchool, @Json)";
            }
            else
            {
                manaPool.Id = existing.Id;
                sql = @"UPDATE CharacterMana SET Json = @Json
                        WHERE CharacterId = @CharacterId AND MagicSchool = @MagicSchool";
            }

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", manaPool.CharacterId);
            command.Parameters.AddWithValue("@MagicSchool", (int)manaPool.MagicSchool);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(manaPool));
            await command.ExecuteNonQueryAsync();

            if (existing == null)
            {
                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = "SELECT last_insert_rowid()";
                long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                if (lastInsertId.HasValue)
                {
                    manaPool.Id = (int)lastInsertId.Value;
                }
            }

            return manaPool;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error saving mana pool for character {manaPool.CharacterId}", ex);
        }
    }

    public async Task UpdateCurrentManaAsync(int characterId, MagicSchool school, int currentMana)
    {
        try
        {
            var pool = await GetManaPoolAsync(characterId, school);
            if (pool != null)
            {
                pool.CurrentMana = currentMana;
                pool.LastUpdated = DateTime.UtcNow;
                await SaveManaPoolAsync(pool);
            }
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error updating current mana for character {characterId}", ex);
        }
    }

    public Task InitializeManaPoolsAsync(int characterId)
    {
        // In a real implementation, this would look up character's mana skills
        // and create pools for each school they have skills in.
        return Task.CompletedTask;
    }
}
