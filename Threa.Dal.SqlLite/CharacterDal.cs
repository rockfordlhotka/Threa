using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;
using Microsoft.Data.Sqlite;

namespace Threa.Dal.Sqlite
{
    public class CharacterDal : ICharacterDal
    {
        private readonly SqliteConnection Connection;

        public CharacterDal(SqliteConnection connection)
        {
            Connection = connection;
            try
            {
                var sql = @"
                CREATE TABLE IF NOT EXISTS 
                Characters (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    PlayerId INTEGER NOT NULL,
                    Json TEXT
                );
            ";
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error creating character table", ex);
            }
        }

        public Character GetBlank()
        {
            return new Character();
        }

        public async Task DeleteCharacterAsync(int id)
        {
            try
            {
                // Remove character from any tables they're attached to
                var removeFromTablesSql = "DELETE FROM TableCharacters WHERE CharacterId = @Id";
                using var removeCommand = Connection.CreateCommand();
                removeCommand.CommandText = removeFromTablesSql;
                removeCommand.Parameters.AddWithValue("@Id", id);
                await removeCommand.ExecuteNonQueryAsync();

                // Delete the character
                var sql = "DELETE FROM Characters WHERE Id = @Id";
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", id);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error deleting character", ex);
            }
        }

        public async Task<Character> GetCharacterAsync(int id)
        {
            try
            {
                var sql = "SELECT Id, PlayerId, Json FROM Characters WHERE Id = @Id";
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", id);
                using var reader = await command.ExecuteReaderAsync();
                if (!reader.Read())
                    throw new NotFoundException($"{nameof(Character)} {id}");
                int characterId = reader.GetInt32(0);
                int playerId = reader.GetInt32(1);
                string json = reader.GetString(2);
                var result = System.Text.Json.JsonSerializer.Deserialize<Character>(json);
                if (result == null)
                    throw new OperationFailedException($"Character {id} not found");
                result.Id = characterId;
                result.PlayerId = playerId;
                return result;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (OperationFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting character", ex);
            }
        }

        public async Task<List<Character>> GetCharactersAsync(int playerId)
        {
            try
            {
                var sql = "SELECT Id, PlayerId, Json FROM Characters WHERE PlayerId = @PlayerId";
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@PlayerId", playerId);
                using var reader = await command.ExecuteReaderAsync();
                List<Character> characters = new();
                while (reader.Read())
                {
                    int characterId = reader.GetInt32(0);
                    int characterPlayerId = reader.GetInt32(1);
                    string json = reader.GetString(2);
                    var obj = System.Text.Json.JsonSerializer.Deserialize<Character>(json);
                    if (obj != null)
                    {
                        obj.Id = characterId;
                        obj.PlayerId = characterPlayerId;
                        characters.Add(obj);
                    }
                }
                return characters;
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting characters", ex);
            }
        }

        public async Task<List<Character>> GetAllCharactersAsync()
        {
            try
            {
                var sql = "SELECT Id, PlayerId, Json FROM Characters";
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                using var reader = await command.ExecuteReaderAsync();
                List<Character> characters = new();
                while (reader.Read())
                {
                    int characterId = reader.GetInt32(0);
                    int playerId = reader.GetInt32(1);
                    string json = reader.GetString(2);
                    var obj = System.Text.Json.JsonSerializer.Deserialize<Character>(json);
                    if (obj != null)
                    {
                        obj.Id = characterId;
                        obj.PlayerId = playerId;
                        characters.Add(obj);
                    }
                }
                return characters;
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting all characters", ex);
            }
        }

        public async Task<List<Character>> GetNpcTemplatesAsync()
        {
            try
            {
                // Reuse existing fetch, filter in memory for JSON storage
                var all = await GetAllCharactersAsync();
                return all.Where(c => c.IsNpc && c.IsTemplate).ToList();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting NPC templates", ex);
            }
        }

        public Task<List<Character>> GetTableNpcsAsync(Guid tableId)
        {
            // Full implementation requires TableDal integration (Phase 25)
            // For Phase 23, provide a stub that throws NotImplementedException
            throw new NotImplementedException("GetTableNpcsAsync requires Phase 25 table integration");
        }

        public async Task<List<string>> GetNpcCategoriesAsync()
        {
            try
            {
                // Reuse existing template fetch, extract distinct categories
                var templates = await GetNpcTemplatesAsync();
                return templates
                    .Where(c => !string.IsNullOrWhiteSpace(c.Category))
                    .Select(c => c.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting NPC categories", ex);
            }
        }

        public async Task<List<Character>> GetArchivedNpcsAsync()
        {
            try
            {
                // Reuse existing fetch, filter in memory for JSON storage
                var all = await GetAllCharactersAsync();
                return all.Where(c => c.IsNpc && !c.IsTemplate && c.IsArchived).ToList();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error getting archived NPCs", ex);
            }
        }

        public async Task<Character> SaveCharacterAsync(Character character)
        {
            try
            {
                string sql;
                using var command = Connection.CreateCommand();

                if (character.Id <= 0)
                {
                    // New character - insert
                    sql = "INSERT INTO Characters (PlayerId, Json) VALUES (@PlayerId, @Json)";
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@PlayerId", character.PlayerId);
                    command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(character));
                    await command.ExecuteNonQueryAsync();

                    using var idCommand = Connection.CreateCommand();
                    idCommand.CommandText = "SELECT last_insert_rowid()";
                    long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                    if (lastInsertId.HasValue)
                    {
                        character.Id = (int)lastInsertId.Value;
                    }
                }
                else
                {
                    // Check if exists
                    using var checkCommand = Connection.CreateCommand();
                    checkCommand.CommandText = "SELECT COUNT(*) FROM Characters WHERE Id = @Id";
                    checkCommand.Parameters.AddWithValue("@Id", character.Id);
                    var exists = (long)(await checkCommand.ExecuteScalarAsync() ?? 0) > 0;

                    if (exists)
                    {
                        sql = "UPDATE Characters SET PlayerId = @PlayerId, Json = @Json WHERE Id = @Id";
                    }
                    else
                    {
                        sql = "INSERT INTO Characters (Id, PlayerId, Json) VALUES (@Id, @PlayerId, @Json)";
                    }
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Id", character.Id);
                    command.Parameters.AddWithValue("@PlayerId", character.PlayerId);
                    command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(character));
                    await command.ExecuteNonQueryAsync();
                }
                return character;
            }
            catch (Exception ex)
            {
                throw new OperationFailedException("Error saving character", ex);
            }
        }
    }
}
