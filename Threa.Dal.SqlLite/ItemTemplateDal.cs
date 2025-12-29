using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of IItemTemplateDal.
/// </summary>
public class ItemTemplateDal : IItemTemplateDal
{
    private readonly SqliteConnection Connection;

    public ItemTemplateDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ItemTemplates (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Json TEXT NOT NULL
                );
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating ItemTemplates table", ex);
        }
    }

    public async Task<List<ItemTemplate>> GetAllTemplatesAsync()
    {
        try
        {
            var sql = "SELECT Json FROM ItemTemplates";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var templates = new List<ItemTemplate>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var template = JsonSerializer.Deserialize<ItemTemplate>(json);
                if (template != null && template.IsActive)
                    templates.Add(template);
            }
            return templates;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting item templates", ex);
        }
    }

    public async Task<List<ItemTemplate>> GetTemplatesByTypeAsync(ItemType itemType)
    {
        var all = await GetAllTemplatesAsync();
        return all.FindAll(t => t.ItemType == itemType);
    }

    public async Task<ItemTemplate> GetTemplateAsync(int id)
    {
        try
        {
            var sql = "SELECT Json FROM ItemTemplates WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"ItemTemplate {id}");
            string json = reader.GetString(0);
            var template = JsonSerializer.Deserialize<ItemTemplate>(json);
            if (template == null)
                throw new OperationFailedException($"ItemTemplate {id} deserialization failed");
            return template;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting item template", ex);
        }
    }

    public async Task<List<ItemTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        var all = await GetAllTemplatesAsync();
        var term = searchTerm.ToLowerInvariant();
        return all.FindAll(t => 
            t.Name.ToLowerInvariant().Contains(term) || 
            t.Description.ToLowerInvariant().Contains(term));
    }

    public async Task<ItemTemplate> SaveTemplateAsync(ItemTemplate template)
    {
        try
        {
            string sql;
            if (template.Id == 0)
            {
                sql = "INSERT INTO ItemTemplates (Json) VALUES (@Json)";
            }
            else
            {
                sql = "UPDATE ItemTemplates SET Json = @Json WHERE Id = @Id";
            }
            
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", template.Id);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(template));
            await command.ExecuteNonQueryAsync();

            if (template.Id == 0)
            {
                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = "SELECT last_insert_rowid()";
                long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                if (lastInsertId.HasValue)
                {
                    template.Id = (int)lastInsertId.Value;
                }
            }
            return template;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving item template", ex);
        }
    }

    public async Task DeactivateTemplateAsync(int id)
    {
        var template = await GetTemplateAsync(id);
        template.IsActive = false;
        await SaveTemplateAsync(template);
    }
}
