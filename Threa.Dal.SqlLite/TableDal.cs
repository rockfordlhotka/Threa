using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

public class TableDal : ITableDal
{
    private readonly SqliteConnection Connection;

    public TableDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTables();
    }

    private void InitializeTables()
    {
        try
        {
            // Create schema version table if it doesn't exist
            var versionTableSql = @"
                CREATE TABLE IF NOT EXISTS SchemaVersion (
                    Version INTEGER NOT NULL PRIMARY KEY,
                    AppliedAt TEXT NOT NULL
                );
            ";
            using var versionCommand = Connection.CreateCommand();
            versionCommand.CommandText = versionTableSql;
            versionCommand.ExecuteNonQuery();

            // Create base tables
            var sql = @"
                CREATE TABLE IF NOT EXISTS GameTables (
                    Id TEXT NOT NULL PRIMARY KEY,
                    GameMasterId INTEGER NOT NULL,
                    Json TEXT
                );

                CREATE TABLE IF NOT EXISTS TableCharacters (
                    TableId TEXT NOT NULL,
                    CharacterId INTEGER NOT NULL,
                    Json TEXT,
                    PRIMARY KEY (TableId, CharacterId)
                );

                CREATE TABLE IF NOT EXISTS TableNpcs (
                    Id TEXT NOT NULL PRIMARY KEY,
                    TableId TEXT NOT NULL,
                    Json TEXT
                );
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            // Run migrations
            RunMigrations();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating game table tables", ex);
        }
    }

    private void RunMigrations()
    {
        var currentVersion = GetCurrentSchemaVersion();

        // Migration 1: Add PlayerId to TableCharacters
        if (currentVersion < 1)
        {
            try
            {
                // Check if PlayerId column already exists
                var checkColumnSql = "SELECT COUNT(*) FROM pragma_table_info('TableCharacters') WHERE name='PlayerId'";
                using var checkCommand = Connection.CreateCommand();
                checkCommand.CommandText = checkColumnSql;
                var columnExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (!columnExists)
                {
                    var migrationSql = "ALTER TABLE TableCharacters ADD COLUMN PlayerId INTEGER NOT NULL DEFAULT 0";
                    using var migrationCommand = Connection.CreateCommand();
                    migrationCommand.CommandText = migrationSql;
                    migrationCommand.ExecuteNonQuery();
                }

                SetSchemaVersion(1);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error running migration 1", ex);
            }
        }

        // Future migrations go here
        // if (currentVersion < 2) { ... }
    }

    private int GetCurrentSchemaVersion()
    {
        try
        {
            var sql = "SELECT COALESCE(MAX(Version), 0) FROM SchemaVersion";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
        catch
        {
            return 0;
        }
    }

    private void SetSchemaVersion(int version)
    {
        var sql = "INSERT OR REPLACE INTO SchemaVersion (Version, AppliedAt) VALUES (@Version, @AppliedAt)";
        using var command = Connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Version", version);
        command.Parameters.AddWithValue("@AppliedAt", DateTime.UtcNow.ToString("o"));
        command.ExecuteNonQuery();
    }

    public GameTable GetBlank()
    {
        return new GameTable
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = TableStatus.Lobby
        };
    }

    public async Task<List<GameTable>> GetTablesByGmAsync(int gameMasterId)
    {
        try
        {
            var sql = "SELECT Json FROM GameTables WHERE GameMasterId = @GameMasterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@GameMasterId", gameMasterId);
            using var reader = await command.ExecuteReaderAsync();
            List<GameTable> tables = new();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var obj = JsonSerializer.Deserialize<GameTable>(json);
                if (obj != null)
                    tables.Add(obj);
            }
            return tables;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting tables by GM", ex);
        }
    }

    public async Task<List<GameTable>> GetActiveTablesAsync()
    {
        try
        {
            var sql = "SELECT Json FROM GameTables";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            List<GameTable> tables = new();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var obj = JsonSerializer.Deserialize<GameTable>(json);
                if (obj != null && obj.Status != TableStatus.Ended)
                    tables.Add(obj);
            }
            return tables;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting active tables", ex);
        }
    }

    public async Task<GameTable> GetTableAsync(Guid id)
    {
        try
        {
            var sql = "SELECT Json FROM GameTables WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"GameTable {id}");
            string json = reader.GetString(0);
            var result = JsonSerializer.Deserialize<GameTable>(json);
            if (result == null)
                throw new OperationFailedException($"GameTable {id} not found");
            return result;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting table", ex);
        }
    }

    public async Task<GameTable> SaveTableAsync(GameTable table)
    {
        try
        {
            // Check if exists
            var checkSql = "SELECT COUNT(*) FROM GameTables WHERE Id = @Id";
            using var checkCommand = Connection.CreateCommand();
            checkCommand.CommandText = checkSql;
            checkCommand.Parameters.AddWithValue("@Id", table.Id.ToString());
            var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

            string sql;
            if (!exists)
            {
                sql = "INSERT INTO GameTables (Id, GameMasterId, Json) VALUES (@Id, @GameMasterId, @Json)";
            }
            else
            {
                sql = "UPDATE GameTables SET GameMasterId = @GameMasterId, Json = @Json WHERE Id = @Id";
            }

            table.LastActivityAt = DateTime.UtcNow;

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", table.Id.ToString());
            command.Parameters.AddWithValue("@GameMasterId", table.GameMasterId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(table));
            await command.ExecuteNonQueryAsync();

            return table;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving table", ex);
        }
    }

    public async Task DeleteTableAsync(Guid id)
    {
        try
        {
            // Delete associated characters and NPCs first
            var deleteCharsSql = "DELETE FROM TableCharacters WHERE TableId = @TableId";
            using var deleteCharsCommand = Connection.CreateCommand();
            deleteCharsCommand.CommandText = deleteCharsSql;
            deleteCharsCommand.Parameters.AddWithValue("@TableId", id.ToString());
            await deleteCharsCommand.ExecuteNonQueryAsync();

            var deleteNpcsSql = "DELETE FROM TableNpcs WHERE TableId = @TableId";
            using var deleteNpcsCommand = Connection.CreateCommand();
            deleteNpcsCommand.CommandText = deleteNpcsSql;
            deleteNpcsCommand.Parameters.AddWithValue("@TableId", id.ToString());
            await deleteNpcsCommand.ExecuteNonQueryAsync();

            // Delete the table
            var sql = "DELETE FROM GameTables WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error deleting table", ex);
        }
    }

    public async Task<TableCharacter> AddCharacterToTableAsync(TableCharacter tableCharacter)
    {
        try
        {
            var sql = "INSERT INTO TableCharacters (TableId, CharacterId, Json) VALUES (@TableId, @CharacterId, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableCharacter.TableId.ToString());
            command.Parameters.AddWithValue("@CharacterId", tableCharacter.CharacterId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(tableCharacter));
            await command.ExecuteNonQueryAsync();
            return tableCharacter;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error adding character to table", ex);
        }
    }

    public async Task UpdateTableCharacterAsync(TableCharacter tableCharacter)
    {
        try
        {
            var sql = "UPDATE TableCharacters SET Json = @Json WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableCharacter.TableId.ToString());
            command.Parameters.AddWithValue("@CharacterId", tableCharacter.CharacterId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(tableCharacter));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating table character", ex);
        }
    }

    public async Task RemoveCharacterFromTableAsync(Guid tableId, int characterId)
    {
        try
        {
            var sql = "DELETE FROM TableCharacters WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            command.Parameters.AddWithValue("@CharacterId", characterId);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error removing character from table", ex);
        }
    }

    public async Task<List<TableCharacter>> GetTableCharactersAsync(Guid tableId)
    {
        try
        {
            // Join with Characters and Players tables to get names
            var sql = @"
                SELECT tc.Json, c.Json as CharJson, p.Json as PlayerJson
                FROM TableCharacters tc
                LEFT JOIN Characters c ON tc.CharacterId = c.Id
                LEFT JOIN Players p ON tc.PlayerId = p.Id
                WHERE tc.TableId = @TableId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            using var reader = await command.ExecuteReaderAsync();
            List<TableCharacter> characters = new();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var obj = JsonSerializer.Deserialize<TableCharacter>(json);
                if (obj != null)
                {
                    // Extract character name from character JSON
                    if (!reader.IsDBNull(1))
                    {
                        string charJson = reader.GetString(1);
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(charJson);
                            if (doc.RootElement.TryGetProperty("Name", out var nameProp))
                            {
                                obj.CharacterName = nameProp.GetString() ?? string.Empty;
                            }
                        }
                        catch { /* Ignore JSON parse errors */ }
                    }

                    // Extract player name from player JSON
                    if (!reader.IsDBNull(2))
                    {
                        string playerJson = reader.GetString(2);
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(playerJson);
                            if (doc.RootElement.TryGetProperty("DisplayName", out var nameProp))
                            {
                                obj.PlayerName = nameProp.GetString();
                            }
                        }
                        catch { /* Ignore JSON parse errors */ }
                    }

                    characters.Add(obj);
                }
            }
            return characters;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting table characters", ex);
        }
    }

    public async Task<TableNpc> AddNpcToTableAsync(TableNpc npc)
    {
        try
        {
            if (npc.Id == Guid.Empty)
                npc.Id = Guid.NewGuid();

            var sql = "INSERT INTO TableNpcs (Id, TableId, Json) VALUES (@Id, @TableId, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", npc.Id.ToString());
            command.Parameters.AddWithValue("@TableId", npc.TableId.ToString());
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(npc));
            await command.ExecuteNonQueryAsync();
            return npc;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error adding NPC to table", ex);
        }
    }

    public async Task UpdateTableNpcAsync(TableNpc npc)
    {
        try
        {
            var sql = "UPDATE TableNpcs SET Json = @Json WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", npc.Id.ToString());
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(npc));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating table NPC", ex);
        }
    }

    public async Task RemoveNpcFromTableAsync(Guid npcId)
    {
        try
        {
            var sql = "DELETE FROM TableNpcs WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", npcId.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error removing NPC from table", ex);
        }
    }

    public async Task<List<TableNpc>> GetTableNpcsAsync(Guid tableId)
    {
        try
        {
            var sql = "SELECT Json FROM TableNpcs WHERE TableId = @TableId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            using var reader = await command.ExecuteReaderAsync();
            List<TableNpc> npcs = new();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var obj = JsonSerializer.Deserialize<TableNpc>(json);
                if (obj != null)
                    npcs.Add(obj);
            }
            return npcs;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting table NPCs", ex);
        }
    }

    public async Task<List<GameTable>> GetTablesForCharacterAsync(int characterId)
    {
        try
        {
            var sql = @"
                SELECT gt.Json FROM GameTables gt
                INNER JOIN TableCharacters tc ON gt.Id = tc.TableId
                WHERE tc.CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();
            List<GameTable> tables = new();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var obj = JsonSerializer.Deserialize<GameTable>(json);
                if (obj != null)
                    tables.Add(obj);
            }
            return tables;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting tables for character", ex);
        }
    }

    public async Task<GameTable?> GetTableForCharacterAsync(int characterId)
    {
        try
        {
            var sql = @"
                SELECT gt.Json FROM GameTables gt
                INNER JOIN TableCharacters tc ON gt.Id = tc.TableId
                WHERE tc.CharacterId = @CharacterId
                LIMIT 1";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                string json = reader.GetString(0);
                return JsonSerializer.Deserialize<GameTable>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting table for character", ex);
        }
    }

    public async Task UpdateGmNotesAsync(Guid tableId, int characterId, string? notes)
    {
        try
        {
            // First fetch the existing table character
            var fetchSql = "SELECT Json FROM TableCharacters WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var fetchCommand = Connection.CreateCommand();
            fetchCommand.CommandText = fetchSql;
            fetchCommand.Parameters.AddWithValue("@TableId", tableId.ToString());
            fetchCommand.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await fetchCommand.ExecuteReaderAsync();

            if (!reader.Read())
                throw new NotFoundException($"TableCharacter {tableId}/{characterId}");

            string json = reader.GetString(0);
            var tableChar = JsonSerializer.Deserialize<TableCharacter>(json);
            if (tableChar == null)
                throw new OperationFailedException($"TableCharacter {tableId}/{characterId} not found");

            reader.Close();

            // Update the GmNotes
            tableChar.GmNotes = notes;

            // Save back
            var updateSql = "UPDATE TableCharacters SET Json = @Json WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var updateCommand = Connection.CreateCommand();
            updateCommand.CommandText = updateSql;
            updateCommand.Parameters.AddWithValue("@TableId", tableId.ToString());
            updateCommand.Parameters.AddWithValue("@CharacterId", characterId);
            updateCommand.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(tableChar));
            await updateCommand.ExecuteNonQueryAsync();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating GM notes", ex);
        }
    }

    public async Task<string?> GetGmNotesAsync(Guid tableId, int characterId)
    {
        try
        {
            var sql = "SELECT Json FROM TableCharacters WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
                return null;

            string json = reader.GetString(0);
            var tableChar = JsonSerializer.Deserialize<TableCharacter>(json);
            return tableChar?.GmNotes;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting GM notes", ex);
        }
    }

    public async Task UpdateCharacterConnectionStatusAsync(Guid tableId, int characterId, ConnectionStatus status, DateTime lastActivity)
    {
        try
        {
            // First fetch the existing table character
            var fetchSql = "SELECT Json FROM TableCharacters WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var fetchCommand = Connection.CreateCommand();
            fetchCommand.CommandText = fetchSql;
            fetchCommand.Parameters.AddWithValue("@TableId", tableId.ToString());
            fetchCommand.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await fetchCommand.ExecuteReaderAsync();

            if (!reader.Read())
                return; // Character not at table - silently ignore

            string json = reader.GetString(0);
            var tableChar = JsonSerializer.Deserialize<TableCharacter>(json);
            if (tableChar == null)
                return; // Invalid data - silently ignore

            reader.Close();

            // Update the connection status and last activity
            tableChar.ConnectionStatus = status;
            tableChar.LastActivity = lastActivity;

            // Save back
            var updateSql = "UPDATE TableCharacters SET Json = @Json WHERE TableId = @TableId AND CharacterId = @CharacterId";
            using var updateCommand = Connection.CreateCommand();
            updateCommand.CommandText = updateSql;
            updateCommand.Parameters.AddWithValue("@TableId", tableId.ToString());
            updateCommand.Parameters.AddWithValue("@CharacterId", characterId);
            updateCommand.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(tableChar));
            await updateCommand.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log but don't throw - connection tracking is not critical
            Console.WriteLine($"[TableDal] Error updating character connection status: {ex.Message}");
        }
    }
}
