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

    /// <summary>
    /// Normalizes a CharacterItem loaded from storage for backward compatibility.
    /// - If EquippedSlot == TwoHand: migrates to EquippedSlots=[MainHand,OffHand], EquippedSlot=MainHand.
    /// - If EquippedSlots is empty and IsEquipped and EquippedSlot != None: populates EquippedSlots=[EquippedSlot].
    /// </summary>
#pragma warning disable CS0618 // TwoHand is obsolete but we need it here for migration
    private static void NormalizeItem(CharacterItem item)
    {
        if (item.EquippedSlot == EquipmentSlot.TwoHand)
        {
            item.EquippedSlots = [EquipmentSlot.MainHand, EquipmentSlot.OffHand];
            item.EquippedSlot = EquipmentSlot.MainHand;
            item.IsEquipped = true;
        }
        else if (item.EquippedSlots.Count == 0 && item.IsEquipped && item.EquippedSlot != EquipmentSlot.None)
        {
            item.EquippedSlots = [item.EquippedSlot];
        }
    }
#pragma warning restore CS0618

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
                    NormalizeItem(item);
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
                    NormalizeItem(item);
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
            NormalizeItem(item);
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

    public async Task EquipItemAsync(Guid itemId, List<EquipmentSlot> slots)
    {
        var item = await GetItemAsync(itemId);

        // Check if item is in a container
        if (item.ContainerItemId.HasValue)
            throw new OperationFailedException("Cannot equip items directly from inside containers. Move to inventory first.");

        // For each incoming slot, unequip any items currently occupying it (except the item being equipped)
        foreach (var slot in slots)
        {
            await UnequipSlotAsync(item.OwnerCharacterId, slot, excludeItemId: itemId);
        }

        item.IsEquipped = true;
        item.EquippedSlots = slots;
        item.EquippedSlot = slots.Count > 0 ? slots[0] : EquipmentSlot.None;
        await UpdateItemAsync(item);
    }

    public async Task UnequipItemAsync(Guid itemId)
    {
        var item = await GetItemAsync(itemId);
        item.IsEquipped = false;
        item.EquippedSlot = EquipmentSlot.None;
        item.EquippedSlots = [];
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
            item.EquippedSlots = [];
        }

        // Validate container exists and is a container
        if (containerItemId.HasValue)
        {
            var container = await GetItemAsync(containerItemId.Value);
            var isValidContainer = container.Template != null &&
                (container.Template.IsContainer ||
                 container.Template.ItemType == ItemType.Container ||
                 container.Template.ItemType == ItemType.AmmoContainer);
            if (!isValidContainer)
                throw new OperationFailedException("Target item is not a container");
        }

        item.ContainerItemId = containerItemId;
        await UpdateItemAsync(item);
    }

    private async Task UnequipSlotAsync(int characterId, EquipmentSlot slot, Guid? excludeItemId = null)
    {
        var items = await GetEquippedItemsAsync(characterId);
        // Find items that occupy this slot — check both the legacy single-slot field and the multi-slot list
        var conflicts = items.FindAll(i =>
            (excludeItemId == null || i.Id != excludeItemId) &&
            (i.EquippedSlot == slot || i.EquippedSlots.Contains(slot)));
        foreach (var conflict in conflicts)
        {
            conflict.IsEquipped = false;
            conflict.EquippedSlot = EquipmentSlot.None;
            conflict.EquippedSlots = [];
            await UpdateItemAsync(conflict);
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
