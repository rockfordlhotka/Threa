using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of ICharacterEffectDal.
/// </summary>
public class CharacterEffectDal : ICharacterEffectDal
{
    private readonly SqliteConnection Connection;
    private readonly IEffectDefinitionDal _definitionDal;

    public CharacterEffectDal(SqliteConnection connection, IEffectDefinitionDal definitionDal)
    {
        Connection = connection;
        _definitionDal = definitionDal;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS CharacterEffects (
                    Id TEXT NOT NULL PRIMARY KEY,
                    CharacterId INTEGER NOT NULL,
                    EffectDefinitionId INTEGER NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_CharacterEffects_CharacterId ON CharacterEffects(CharacterId);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating CharacterEffects table", ex);
        }
    }

    public async Task<List<CharacterEffect>> GetCharacterEffectsAsync(int characterId)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterEffects WHERE CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            using var reader = await command.ExecuteReaderAsync();
            var effects = new List<CharacterEffect>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var effect = JsonSerializer.Deserialize<CharacterEffect>(json);
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
            throw new OperationFailedException("Error getting character effects", ex);
        }
    }

    public async Task<List<CharacterEffect>> GetCharacterEffectsByTypeAsync(int characterId, EffectType effectType)
    {
        var effects = await GetCharacterEffectsAsync(characterId);
        return effects.Where(e => e.Definition?.EffectType == effectType).ToList();
    }

    public async Task<CharacterEffect?> GetEffectAsync(Guid id)
    {
        try
        {
            var sql = "SELECT Json FROM CharacterEffects WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            var effect = JsonSerializer.Deserialize<CharacterEffect>(json);
            if (effect != null)
            {
                effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
            }
            return effect;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting character effect", ex);
        }
    }

    public async Task<bool> HasEffectAsync(int characterId, string effectName)
    {
        var effects = await GetCharacterEffectsAsync(characterId);
        return effects.Any(e => e.Definition?.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase) == true);
    }

    public async Task<List<CharacterEffect>> GetEffectsByNameAsync(int characterId, string effectName)
    {
        var effects = await GetCharacterEffectsAsync(characterId);
        return effects.Where(e => e.Definition?.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase) == true).ToList();
    }

    public async Task<CharacterEffect> ApplyEffectAsync(CharacterEffect effect)
    {
        var definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        effect.Definition = definition;

        var existingEffects = await GetEffectsByNameAsync(effect.CharacterId, definition.Name);

        if (existingEffects.Count > 0 && !definition.IsStackable)
        {
            switch (definition.StackBehavior)
            {
                case StackBehavior.Replace:
                    foreach (var existing in existingEffects)
                    {
                        await RemoveEffectAsync(existing.Id);
                    }
                    break;

                case StackBehavior.Extend:
                    var toExtend = existingEffects.First();
                    if (toExtend.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                        toExtend.RoundsRemaining += effect.RoundsRemaining;
                    if (toExtend.EndTime.HasValue && effect.EndTime.HasValue)
                        toExtend.EndTime = toExtend.EndTime.Value.Add(effect.EndTime.Value - effect.StartTime);
                    await UpdateEffectAsync(toExtend);
                    return toExtend;

                case StackBehavior.Intensify:
                    var toIntensify = existingEffects.First();
                    if (toIntensify.CurrentStacks < definition.MaxStacks)
                        toIntensify.CurrentStacks++;
                    await UpdateEffectAsync(toIntensify);
                    return toIntensify;

                case StackBehavior.Independent:
                    break;
            }
        }
        else if (existingEffects.Count > 0 && definition.IsStackable)
        {
            if (existingEffects.Count >= definition.MaxStacks)
            {
                switch (definition.StackBehavior)
                {
                    case StackBehavior.Replace:
                        var oldest = existingEffects.OrderBy(e => e.StartTime).First();
                        await RemoveEffectAsync(oldest.Id);
                        break;
                    case StackBehavior.Extend:
                    case StackBehavior.Intensify:
                        var mostRecent = existingEffects.OrderByDescending(e => e.StartTime).First();
                        if (definition.StackBehavior == StackBehavior.Extend)
                        {
                            if (mostRecent.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                                mostRecent.RoundsRemaining += effect.RoundsRemaining;
                        }
                        else
                        {
                            mostRecent.CurrentStacks = Math.Min(mostRecent.CurrentStacks + 1, definition.MaxStacks);
                        }
                        await UpdateEffectAsync(mostRecent);
                        return mostRecent;
                    case StackBehavior.Independent:
                        return existingEffects.First();
                }
            }
        }

        if (effect.Id == Guid.Empty)
            effect.Id = Guid.NewGuid();

        try
        {
            var sql = "INSERT INTO CharacterEffects (Id, CharacterId, EffectDefinitionId, Json) VALUES (@Id, @CharacterId, @EffectDefinitionId, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effect.Id.ToString());
            command.Parameters.AddWithValue("@CharacterId", effect.CharacterId);
            command.Parameters.AddWithValue("@EffectDefinitionId", effect.EffectDefinitionId);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
            await command.ExecuteNonQueryAsync();
            return effect;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error applying character effect", ex);
        }
    }

    public async Task<CharacterEffect> UpdateEffectAsync(CharacterEffect effect)
    {
        try
        {
            var sql = "UPDATE CharacterEffects SET Json = @Json WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effect.Id.ToString());
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
            var rows = await command.ExecuteNonQueryAsync();
            if (rows == 0)
                throw new NotFoundException($"CharacterEffect {effect.Id}");
            return effect;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error updating character effect", ex);
        }
    }

    public async Task RemoveEffectAsync(Guid id)
    {
        try
        {
            var sql = "DELETE FROM CharacterEffects WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error removing character effect", ex);
        }
    }

    public async Task RemoveEffectsByTypeAsync(int characterId, EffectType effectType)
    {
        var effects = await GetCharacterEffectsByTypeAsync(characterId, effectType);
        foreach (var effect in effects)
        {
            await RemoveEffectAsync(effect.Id);
        }
    }

    public async Task RemoveExpiredEffectsAsync(int characterId)
    {
        var now = DateTime.UtcNow;
        var effects = await GetCharacterEffectsAsync(characterId);
        var expired = effects.Where(e => e.EndTime.HasValue && e.EndTime < now).ToList();
        foreach (var effect in expired)
        {
            await RemoveEffectAsync(effect.Id);
        }
    }

    public async Task<List<CharacterEffect>> ProcessEndOfRoundAsync(int characterId)
    {
        var expiredEffects = new List<CharacterEffect>();
        var effects = await GetCharacterEffectsAsync(characterId);

        foreach (var effect in effects.ToList())
        {
            bool updated = false;

            if (effect.RoundsRemaining.HasValue)
            {
                effect.RoundsRemaining--;
                updated = true;
                if (effect.RoundsRemaining <= 0)
                {
                    expiredEffects.Add(effect);
                    await RemoveEffectAsync(effect.Id);
                    continue;
                }
            }

            if (effect.RoundsUntilTick.HasValue)
            {
                effect.RoundsUntilTick--;
                updated = true;
                if (effect.RoundsUntilTick <= 0)
                {
                    var dotImpact = effect.Definition?.Impacts
                        .FirstOrDefault(i => i.ImpactType == EffectImpactType.DamageOverTime);
                    if (dotImpact?.DamageInterval != null)
                    {
                        effect.RoundsUntilTick = dotImpact.DamageInterval;
                    }
                }
            }

            if (updated)
            {
                await UpdateEffectAsync(effect);
            }
        }

        return expiredEffects;
    }
}
