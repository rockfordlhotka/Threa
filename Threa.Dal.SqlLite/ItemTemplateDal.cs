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

    /// <summary>
    /// Normalizes a template loaded from storage for backward compatibility.
    /// - If EquipmentSlot == TwoHand: migrates to EquipmentSlots=[MainHand,OffHand], OccupiesAllSlots=true, EquipmentSlot=MainHand.
    /// - If EquipmentSlots is empty and EquipmentSlot != None: populates EquipmentSlots=[EquipmentSlot].
    /// </summary>
#pragma warning disable CS0618 // TwoHand is obsolete but we need it here for migration
    private static void NormalizeTemplate(ItemTemplate template)
    {
        if (template.EquipmentSlot == EquipmentSlot.TwoHand)
        {
            template.EquipmentSlots = [EquipmentSlot.MainHand, EquipmentSlot.OffHand];
            template.OccupiesAllSlots = true;
            template.EquipmentSlot = EquipmentSlot.MainHand;
        }
        else if (template.EquipmentSlots.Count == 0 && template.EquipmentSlot != EquipmentSlot.None)
        {
            template.EquipmentSlots = [template.EquipmentSlot];
        }
    }
#pragma warning restore CS0618

    public async Task<List<ItemTemplate>> GetAllTemplatesAsync()
    {
        try
        {
            var sql = "SELECT Id, Json FROM ItemTemplates";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var templates = new List<ItemTemplate>();
            while (reader.Read())
            {
                int dbId = reader.GetInt32(0);
                string json = reader.GetString(1);
                var template = JsonSerializer.Deserialize<ItemTemplate>(json);
                if (template != null && template.IsActive)
                {
                    // Ensure the ID from the database is set
                    template.Id = dbId;
                    NormalizeTemplate(template);
                    templates.Add(template);
                }
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
            var sql = "SELECT Id, Json FROM ItemTemplates WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"ItemTemplate {id}");
            int dbId = reader.GetInt32(0);
            string json = reader.GetString(1);
            var template = JsonSerializer.Deserialize<ItemTemplate>(json);
            if (template == null)
                throw new OperationFailedException($"ItemTemplate {id} deserialization failed");
            // Ensure the ID from the database is set
            template.Id = dbId;
            NormalizeTemplate(template);
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
            // Sync legacy EquipmentSlot from EquipmentSlots before saving
            if (template.EquipmentSlots.Count > 0)
                template.EquipmentSlot = template.EquipmentSlots[0];
            else if (template.EquipmentSlot != EquipmentSlot.None)
                template.EquipmentSlots = [template.EquipmentSlot];

            string sql;
            using var command = Connection.CreateCommand();

            if (template.Id <= 0)
            {
                // New template - insert
                sql = "INSERT INTO ItemTemplates (Json) VALUES (@Json)";
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(template));
                await command.ExecuteNonQueryAsync();

                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = "SELECT last_insert_rowid()";
                long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                if (lastInsertId.HasValue)
                {
                    template.Id = (int)lastInsertId.Value;
                }
            }
            else
            {
                // Check if exists
                using var checkCommand = Connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM ItemTemplates WHERE Id = @Id";
                checkCommand.Parameters.AddWithValue("@Id", template.Id);
                var exists = (long)(await checkCommand.ExecuteScalarAsync() ?? 0) > 0;

                if (exists)
                {
                    sql = "UPDATE ItemTemplates SET Json = @Json WHERE Id = @Id";
                }
                else
                {
                    sql = "INSERT INTO ItemTemplates (Id, Json) VALUES (@Id, @Json)";
                }
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", template.Id);
                command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(template));
                await command.ExecuteNonQueryAsync();
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
