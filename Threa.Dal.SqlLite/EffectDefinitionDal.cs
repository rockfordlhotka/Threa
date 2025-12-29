using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of IEffectDefinitionDal.
/// </summary>
public class EffectDefinitionDal : IEffectDefinitionDal
{
    private readonly SqliteConnection Connection;

    public EffectDefinitionDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS EffectDefinitions (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_EffectDefinitions_Name ON EffectDefinitions(Name);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating EffectDefinitions table", ex);
        }
    }

    public async Task<List<EffectDefinition>> GetAllDefinitionsAsync()
    {
        try
        {
            var sql = "SELECT Json FROM EffectDefinitions";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var definitions = new List<EffectDefinition>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var definition = JsonSerializer.Deserialize<EffectDefinition>(json);
                if (definition != null && definition.IsActive)
                    definitions.Add(definition);
            }
            return definitions;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect definitions", ex);
        }
    }

    public async Task<List<EffectDefinition>> GetDefinitionsByTypeAsync(EffectType effectType)
    {
        var all = await GetAllDefinitionsAsync();
        return all.FindAll(d => d.EffectType == effectType);
    }

    public async Task<EffectDefinition> GetDefinitionAsync(int id)
    {
        try
        {
            var sql = "SELECT Json FROM EffectDefinitions WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"EffectDefinition {id}");
            string json = reader.GetString(0);
            var definition = JsonSerializer.Deserialize<EffectDefinition>(json);
            if (definition == null)
                throw new OperationFailedException($"EffectDefinition {id} deserialization failed");
            return definition;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect definition", ex);
        }
    }

    public async Task<EffectDefinition?> GetDefinitionByNameAsync(string name)
    {
        try
        {
            var sql = "SELECT Json FROM EffectDefinitions WHERE Name = @Name COLLATE NOCASE";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Name", name);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            var definition = JsonSerializer.Deserialize<EffectDefinition>(json);
            return definition?.IsActive == true ? definition : null;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect definition by name", ex);
        }
    }

    public async Task<List<EffectDefinition>> SearchDefinitionsAsync(string searchTerm)
    {
        var all = await GetAllDefinitionsAsync();
        var term = searchTerm.ToLowerInvariant();
        return all.FindAll(d =>
            d.Name.ToLowerInvariant().Contains(term) ||
            d.Description.ToLowerInvariant().Contains(term));
    }

    public async Task<EffectDefinition> SaveDefinitionAsync(EffectDefinition definition)
    {
        try
        {
            string sql;
            if (definition.Id == 0)
            {
                sql = "INSERT INTO EffectDefinitions (Name, Json) VALUES (@Name, @Json)";
            }
            else
            {
                sql = "UPDATE EffectDefinitions SET Name = @Name, Json = @Json WHERE Id = @Id";
            }

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", definition.Id);
            command.Parameters.AddWithValue("@Name", definition.Name);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(definition));
            await command.ExecuteNonQueryAsync();

            if (definition.Id == 0)
            {
                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = "SELECT last_insert_rowid()";
                long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                if (lastInsertId.HasValue)
                {
                    definition.Id = (int)lastInsertId.Value;
                }
            }
            return definition;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving effect definition", ex);
        }
    }

    public async Task DeactivateDefinitionAsync(int id)
    {
        var definition = await GetDefinitionAsync(id);
        definition.IsActive = false;
        await SaveDefinitionAsync(definition);
    }
}
