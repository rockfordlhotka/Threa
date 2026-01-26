using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of ICharacterItemDal.
/// </summary>
public class CharacterItemDal : ICharacterItemDal
{
    private readonly SqliteConnection Connection;
    private readonly IItemTemplateDal _templateDal;

    public CharacterItemDal(SqliteConnection connection, IItemTemplateDal templateDal)
    {
        Connection = connection;
        _templateDal = templateDal;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS CharacterItems (
                    Id TEXT NOT NULL PRIMARY KEY,
                    OwnerCharacterId INTEGER NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_CharacterItems_Owner ON CharacterItems(OwnerCharacterId);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating CharacterItems table", ex);
        }
    }

    public async Task<List<CharacterItem>> GetCharacterItemsAsync(int characterId)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterItems WHERE OwnerCharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();
            var items = new List<CharacterItem>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var item = JsonSerializer.Deserialize<CharacterItem>(json);
                if (item != null)
                {
                    await PopulateTemplateAsync(item);
                    items.Add(item);
                }
            }
            return items;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting character items", ex);
        }
    }

    public async Task<List<CharacterItem>> GetEquippedItemsAsync(int characterId)
    {
        var all = await GetCharacterItemsAsync(characterId);
        return all.FindAll(i => i.IsEquipped);
    }

    public async Task<List<CharacterItem>> GetContainerContentsAsync(Guid containerItemId)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterItems";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var items = new List<CharacterItem>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var item = JsonSerializer.Deserialize<CharacterItem>(json);
                if (item != null && item.ContainerItemId == containerItemId)
                {
                    await PopulateTemplateAsync(item);
                    items.Add(item);
                }
            }
            return items;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting container contents", ex);
        }
    }

    public async Task<CharacterItem> GetItemAsync(Guid id)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterItems WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"CharacterItem {id}");
            string json = reader.GetString(0);
            var item = JsonSerializer.Deserialize<CharacterItem>(json);
            if (item == null)
                throw new OperationFailedException($"CharacterItem {id} deserialization failed");
            await PopulateTemplateAsync(item);
            return item;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting item", ex);
        }
    }

    public async Task<CharacterItem> AddItemAsync(CharacterItem item)
    {
        try
        {
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;

            var sql = "INSERT INTO CharacterItems (Id, OwnerCharacterId, Json) VALUES (@Id, @OwnerId, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", item.Id.ToString());
            command.Parameters.AddWithValue("@OwnerId", item.OwnerCharacterId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(item));
            await command.ExecuteNonQueryAsync();
            
            await PopulateTemplateAsync(item);
            return item;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error adding item", ex);
        }
    }

    public async Task<CharacterItem> UpdateItemAsync(CharacterItem item)
    {
        try
        {
            var sql = "UPDATE CharacterItems SET OwnerCharacterId = @OwnerId, Json = @Json WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", item.Id.ToString());
            command.Parameters.AddWithValue("@OwnerId", item.OwnerCharacterId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(item));
            var affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                throw new NotFoundException($"CharacterItem {item.Id}");
            
            await PopulateTemplateAsync(item);
            return item;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating item", ex);
        }
    }

    public async Task DeleteItemAsync(Guid id)
    {
        try
        {
            var sql = "DELETE FROM CharacterItems WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            var affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                throw new NotFoundException($"CharacterItem {id}");
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error deleting item", ex);
        }
    }

    public async Task EquipItemAsync(Guid itemId, EquipmentSlot slot)
    {
        var item = await GetItemAsync(itemId);
        
        // Check if item is in a container
        if (item.ContainerItemId.HasValue)
            throw new OperationFailedException("Cannot equip items directly from inside containers. Move to inventory first.");

        // Handle two-handed weapon equipping
        if (slot == EquipmentSlot.TwoHand)
        {
            await UnequipSlotAsync(item.OwnerCharacterId, EquipmentSlot.MainHand);
            await UnequipSlotAsync(item.OwnerCharacterId, EquipmentSlot.OffHand);
        }
        else if (slot == EquipmentSlot.MainHand || slot == EquipmentSlot.OffHand)
        {
            await UnequipSlotAsync(item.OwnerCharacterId, EquipmentSlot.TwoHand);
        }

        // Unequip any item currently in the target slot
        await UnequipSlotAsync(item.OwnerCharacterId, slot);

        item.IsEquipped = true;
        item.EquippedSlot = slot;
        await UpdateItemAsync(item);
    }

    public async Task UnequipItemAsync(Guid itemId)
    {
        var item = await GetItemAsync(itemId);
        item.IsEquipped = false;
        item.EquippedSlot = EquipmentSlot.None;
        await UpdateItemAsync(item);
    }

    public async Task MoveToContainerAsync(Guid itemId, Guid? containerItemId)
    {
        var item = await GetItemAsync(itemId);
        
        // If equipping, must unequip first
        if (item.IsEquipped && containerItemId.HasValue)
        {
            item.IsEquipped = false;
            item.EquippedSlot = EquipmentSlot.None;
        }

        // Validate container exists and is a container
        if (containerItemId.HasValue)
        {
            var container = await GetItemAsync(containerItemId.Value);
            if (container.Template == null || !container.Template.IsContainer)
                throw new OperationFailedException("Target item is not a container");
        }

        item.ContainerItemId = containerItemId;
        await UpdateItemAsync(item);
    }

    private async Task UnequipSlotAsync(int characterId, EquipmentSlot slot)
    {
        var items = await GetEquippedItemsAsync(characterId);
        var equippedItem = items.Find(i => i.EquippedSlot == slot);
        if (equippedItem != null)
        {
            equippedItem.IsEquipped = false;
            equippedItem.EquippedSlot = EquipmentSlot.None;
            await UpdateItemAsync(equippedItem);
        }
    }

    public async Task<List<CharacterItem>> GetEquippedItemsWithTemplatesAsync(int characterId)
    {
        var items = await GetEquippedItemsAsync(characterId);
        // Filter to only items that have valid templates
        return items.Where(i => i.Template != null).ToList();
    }

    private async Task PopulateTemplateAsync(CharacterItem item)
    {
        try
        {
            item.Template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
        }
        catch (NotFoundException)
        {
            // Template not found, leave as null
            item.Template = null;
        }
    }
}
