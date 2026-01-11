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

        public async Task<Character> SaveCharacterAsync(Character character)
        {
            try
            {
                string sql;
                if (character.Id == 0)
                {
                    sql = "INSERT INTO Characters (PlayerId, Json) VALUES (@PlayerId, @Json)";
                }
                else
                {
                    sql = "UPDATE Characters SET PlayerId = @PlayerId, Json = @Json WHERE Id = @Id";
                }
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", character.Id);
                command.Parameters.AddWithValue("@PlayerId", character.PlayerId);
                command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(character));
                await command.ExecuteNonQueryAsync();

                if (character.Id == 0)
                {
                    sql = "SELECT last_insert_rowid()";
                    using var idCommand = Connection.CreateCommand();
                    idCommand.CommandText = sql;
                    long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                    if (lastInsertId.HasValue)
                    {
                        character.Id = (int)lastInsertId.Value;
                    }
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
