using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of IItemEffectDal.
/// </summary>
public class ItemEffectDal : IItemEffectDal
{
    private readonly SqliteConnection Connection;
    private readonly IEffectDefinitionDal _definitionDal;
    private readonly ICharacterItemDal _itemDal;

    public ItemEffectDal(SqliteConnection connection, IEffectDefinitionDal definitionDal, ICharacterItemDal itemDal)
    {
        Connection = connection;
        _definitionDal = definitionDal;
        _itemDal = itemDal;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ItemEffects (
                    Id TEXT NOT NULL PRIMARY KEY,
                    ItemId TEXT NOT NULL,
                    EffectDefinitionId INTEGER NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_ItemEffects_ItemId ON ItemEffects(ItemId);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating ItemEffects table", ex);
        }
    }

    public async Task<List<ItemEffect>> GetItemEffectsAsync(Guid itemId)
    {
        try
        {
            var sql = "SELECT Json FROM ItemEffects WHERE ItemId = @ItemId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@ItemId", itemId.ToString());
            using var reader = await command.ExecuteReaderAsync();
            var effects = new List<ItemEffect>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var effect = JsonSerializer.Deserialize<ItemEffect>(json);
                if (effect != null && effect.IsActive)
                {
                    effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
                    effects.Add(effect);
                }
            }
            return effects;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting item effects", ex);
        }
    }

    public async Task<List<ItemEffect>> GetAllItemEffectsForCharacterAsync(int characterId)
    {
        var items = await _itemDal.GetCharacterItemsAsync(characterId);
        var allEffects = new List<ItemEffect>();
        foreach (var item in items)
        {
            var effects = await GetItemEffectsAsync(item.Id);
            foreach (var effect in effects)
            {
                effect.Item = item;
                allEffects.Add(effect);
            }
        }
        return allEffects;
    }

    public async Task<List<ItemEffect>> GetEquippedItemEffectsAsync(int characterId)
    {
        var items = await _itemDal.GetEquippedItemsAsync(characterId);
        var allEffects = new List<ItemEffect>();
        foreach (var item in items)
        {
            var effects = await GetItemEffectsAsync(item.Id);
            foreach (var effect in effects)
            {
                effect.Item = item;
                allEffects.Add(effect);
            }
        }
        return allEffects;
    }

    public async Task<ItemEffect?> GetEffectAsync(Guid id)
    {
        try
        {
            var sql = "SELECT Json FROM ItemEffects WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            var effect = JsonSerializer.Deserialize<ItemEffect>(json);
            if (effect != null)
            {
                effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
            }
            return effect;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting item effect", ex);
        }
    }

    public async Task<ItemEffect> ApplyEffectAsync(ItemEffect effect)
    {
        var definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        effect.Definition = definition;

        var existingEffects = await GetItemEffectsAsync(effect.ItemId);
        var matchingEffects = existingEffects.Where(e => e.EffectDefinitionId == effect.EffectDefinitionId).ToList();

        if (matchingEffects.Count > 0 && !definition.IsStackable)
        {
            switch (definition.StackBehavior)
            {
                case StackBehavior.Replace:
                    foreach (var existing in matchingEffects)
                    {
                        await RemoveEffectAsync(existing.Id);
                    }
                    break;

                case StackBehavior.Extend:
                    var toExtend = matchingEffects.First();
                    if (toExtend.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                        toExtend.RoundsRemaining += effect.RoundsRemaining;
                    await UpdateEffectAsync(toExtend);
                    return toExtend;

                case StackBehavior.Intensify:
                    var toIntensify = matchingEffects.First();
                    if (toIntensify.CurrentStacks < definition.MaxStacks)
                        toIntensify.CurrentStacks++;
                    await UpdateEffectAsync(toIntensify);
                    return toIntensify;

                case StackBehavior.Independent:
                    break;
            }
        }

        if (effect.Id == Guid.Empty)
            effect.Id = Guid.NewGuid();

        try
        {
            var sql = "INSERT INTO ItemEffects (Id, ItemId, EffectDefinitionId, Json) VALUES (@Id, @ItemId, @EffectDefinitionId, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effect.Id.ToString());
            command.Parameters.AddWithValue("@ItemId", effect.ItemId.ToString());
            command.Parameters.AddWithValue("@EffectDefinitionId", effect.EffectDefinitionId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
            await command.ExecuteNonQueryAsync();
            return effect;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error applying item effect", ex);
        }
    }

    public async Task<ItemEffect> UpdateEffectAsync(ItemEffect effect)
    {
        try
        {
            var sql = "UPDATE ItemEffects SET Json = @Json WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effect.Id.ToString());
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
            var rows = await command.ExecuteNonQueryAsync();
            if (rows == 0)
                throw new NotFoundException($"ItemEffect {effect.Id}");
            return effect;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating item effect", ex);
        }
    }

    public async Task RemoveEffectAsync(Guid id)
    {
        try
        {
            var sql = "DELETE FROM ItemEffects WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error removing item effect", ex);
        }
    }

    public async Task RemoveAllEffectsAsync(Guid itemId)
    {
        try
        {
            var sql = "DELETE FROM ItemEffects WHERE ItemId = @ItemId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@ItemId", itemId.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error removing all item effects", ex);
        }
    }

    public async Task RemoveExpiredEffectsAsync(int characterId)
    {
        var now = DateTime.UtcNow;
        var effects = await GetAllItemEffectsForCharacterAsync(characterId);
        var expired = effects.Where(e => e.EndTime.HasValue && e.EndTime < now).ToList();
        foreach (var effect in expired)
        {
            await RemoveEffectAsync(effect.Id);
        }
    }

    public async Task<List<ItemEffect>> ProcessEndOfRoundAsync(int characterId)
    {
        var expiredEffects = new List<ItemEffect>();
        var effects = await GetAllItemEffectsForCharacterAsync(characterId);

        foreach (var effect in effects.ToList())
        {
            if (effect.RoundsRemaining.HasValue)
            {
                effect.RoundsRemaining--;
                if (effect.RoundsRemaining <= 0)
                {
                    expiredEffects.Add(effect);
                    await RemoveEffectAsync(effect.Id);
                }
                else
                {
                    await UpdateEffectAsync(effect);
                }
            }
        }

        return expiredEffects;
    }
}
