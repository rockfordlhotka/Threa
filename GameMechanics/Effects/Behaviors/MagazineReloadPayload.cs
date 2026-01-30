using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for magazine reload deferred action.
/// Stores all data needed to complete the reload when concentration finishes.
/// </summary>
public class MagazineReloadPayload
{
    /// <summary>
    /// The weapon CharacterItem ID being reloaded.
    /// </summary>
    [JsonPropertyName("weaponItemId")]
    public Guid WeaponItemId { get; set; }

    /// <summary>
    /// The magazine/ammo source CharacterItem ID.
    /// </summary>
    [JsonPropertyName("magazineItemId")]
    public Guid MagazineItemId { get; set; }

    /// <summary>
    /// Number of rounds to load into the weapon.
    /// </summary>
    [JsonPropertyName("roundsToLoad")]
    public int RoundsToLoad { get; set; }

    /// <summary>
    /// Serializes this payload to JSON for storage in ConcentrationState.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// Deserializes a payload from JSON.
    /// </summary>
    public static MagazineReloadPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<MagazineReloadPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
