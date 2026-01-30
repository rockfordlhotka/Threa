using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of ILocationEffectDal.
/// </summary>
public class LocationEffectDal : ILocationEffectDal
{
    private readonly SqliteConnection Connection;

    public LocationEffectDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTables();
    }

    private void InitializeTables()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS SpellLocations (
                    Id TEXT NOT NULL PRIMARY KEY,
                    CampaignId INTEGER,
                    Name TEXT NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_SpellLocations_CampaignId ON SpellLocations(CampaignId);

                CREATE TABLE IF NOT EXISTS LocationEffects (
                    Id TEXT NOT NULL PRIMARY KEY,
                    LocationId TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    Json TEXT NOT NULL,
                    FOREIGN KEY (LocationId) REFERENCES SpellLocations(Id)
                );
                CREATE INDEX IF NOT EXISTS IX_LocationEffects_LocationId ON LocationEffects(LocationId);
                CREATE INDEX IF NOT EXISTS IX_LocationEffects_IsActive ON LocationEffects(IsActive);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating SpellLocations/LocationEffects tables", ex);
        }
    }

    public async Task<SpellLocation> CreateLocationAsync(SpellLocation location)
    {
        try
        {
            if (location.Id == Guid.Empty)
            {
                location.Id = Guid.NewGuid();
            }

            var sql = @"INSERT INTO SpellLocations (Id, CampaignId, Name, Json)
                        VALUES (@Id, @CampaignId, @Name, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", location.Id.ToString());
            command.Parameters.AddWithValue("@CampaignId", location.CampaignId.HasValue ? (object)location.CampaignId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Name", location.Name);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(location));
            await command.ExecuteNonQueryAsync();

            return location;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating spell location", ex);
        }
    }

    public async Task<SpellLocation?> GetLocationAsync(Guid locationId)
    {
        try
        {
            var sql = "SELECT Json FROM SpellLocations WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", locationId.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<SpellLocation>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting location {locationId}", ex);
        }
    }

    public async Task<List<SpellLocation>> GetCampaignLocationsAsync(int campaignId)
    {
        try
        {
            var sql = "SELECT Json FROM SpellLocations WHERE CampaignId = @CampaignId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CampaignId", campaignId);
            using var reader = await command.ExecuteReaderAsync();
            var locations = new List<SpellLocation>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var location = JsonSerializer.Deserialize<SpellLocation>(json);
                if (location != null)
                    locations.Add(location);
            }
            return locations;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting locations for campaign {campaignId}", ex);
        }
    }

    public async Task<LocationEffect> CreateLocationEffectAsync(LocationEffect effect)
    {
        try
        {
            if (effect.Id == Guid.Empty)
            {
                effect.Id = Guid.NewGuid();
            }

            var sql = @"INSERT INTO LocationEffects (Id, LocationId, IsActive, Json)
                        VALUES (@Id, @LocationId, @IsActive, @Json)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effect.Id.ToString());
            command.Parameters.AddWithValue("@LocationId", effect.LocationId.ToString());
            command.Parameters.AddWithValue("@IsActive", effect.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
            await command.ExecuteNonQueryAsync();

            return effect;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating location effect", ex);
        }
    }

    public async Task<List<LocationEffect>> GetActiveEffectsAtLocationAsync(Guid locationId)
    {
        try
        {
            var sql = "SELECT Json FROM LocationEffects WHERE LocationId = @LocationId AND IsActive = 1";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@LocationId", locationId.ToString());
            using var reader = await command.ExecuteReaderAsync();
            var effects = new List<LocationEffect>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var effect = JsonSerializer.Deserialize<LocationEffect>(json);
                if (effect != null)
                    effects.Add(effect);
            }
            return effects;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting effects at location {locationId}", ex);
        }
    }

    public async Task<List<LocationEffect>> GetActiveCampaignEffectsAsync(int campaignId)
    {
        try
        {
            var sql = @"
                SELECT le.Json FROM LocationEffects le
                INNER JOIN SpellLocations sl ON le.LocationId = sl.Id
                WHERE sl.CampaignId = @CampaignId AND le.IsActive = 1";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CampaignId", campaignId);
            using var reader = await command.ExecuteReaderAsync();
            var effects = new List<LocationEffect>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var effect = JsonSerializer.Deserialize<LocationEffect>(json);
                if (effect != null)
                    effects.Add(effect);
            }
            return effects;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting active effects for campaign {campaignId}", ex);
        }
    }

    public async Task<List<LocationEffect>> ProcessRoundAdvanceAsync(int campaignId)
    {
        try
        {
            // Get all active effects for this campaign
            var activeEffects = await GetActiveCampaignEffectsAsync(campaignId);
            var expiredEffects = new List<LocationEffect>();

            foreach (var effect in activeEffects)
            {
                if (effect.RoundsRemaining.HasValue)
                {
                    effect.RoundsRemaining--;
                    if (effect.RoundsRemaining <= 0)
                    {
                        effect.IsActive = false;
                        expiredEffects.Add(effect);
                    }

                    // Update the effect in the database
                    await UpdateLocationEffectAsync(effect);
                }
            }

            return expiredEffects;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error processing round advance for campaign {campaignId}", ex);
        }
    }

    private async Task UpdateLocationEffectAsync(LocationEffect effect)
    {
        var sql = @"UPDATE LocationEffects SET IsActive = @IsActive, Json = @Json WHERE Id = @Id";
        using var command = Connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Id", effect.Id.ToString());
        command.Parameters.AddWithValue("@IsActive", effect.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(effect));
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeactivateEffectAsync(Guid effectId)
    {
        try
        {
            var sql = @"UPDATE LocationEffects SET IsActive = 0,
                        Json = json_set(Json, '$.IsActive', json('false'))
                        WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", effectId.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error deactivating effect {effectId}", ex);
        }
    }

    public async Task CleanupExpiredEffectsAsync(int campaignId)
    {
        try
        {
            var sql = @"
                DELETE FROM LocationEffects
                WHERE LocationId IN (SELECT Id FROM SpellLocations WHERE CampaignId = @CampaignId)
                AND IsActive = 0";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CampaignId", campaignId);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error cleaning up expired effects for campaign {campaignId}", ex);
        }
    }
}
