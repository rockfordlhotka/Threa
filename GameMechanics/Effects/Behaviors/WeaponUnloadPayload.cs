using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for weapon unload deferred action.
/// Stores all data needed to complete unloading ammo from a weapon to inventory when concentration finishes.
/// </summary>
public class WeaponUnloadPayload
{
    /// <summary>
    /// The weapon CharacterItem ID being unloaded.
    /// </summary>
    [JsonPropertyName("weaponItemId")]
    public Guid WeaponItemId { get; set; }

    /// <summary>
    /// The character ID who owns the weapon.
    /// </summary>
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }

    /// <summary>
    /// Number of rounds to unload from the weapon.
    /// </summary>
    [JsonPropertyName("roundsToUnload")]
    public int RoundsToUnload { get; set; }

    /// <summary>
    /// Type of ammo being unloaded (e.g., "9mm", "Arrow").
    /// </summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>
    /// Display name of the weapon for messages.
    /// </summary>
    [JsonPropertyName("weaponName")]
    public string? WeaponName { get; set; }

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
    public static WeaponUnloadPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<WeaponUnloadPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
