using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for ammo container unload deferred action.
/// Stores all data needed to complete unloading rounds from a container back to inventory when concentration finishes.
/// </summary>
public class AmmoContainerUnloadPayload
{
    /// <summary>
    /// The ammo container CharacterItem ID being unloaded (magazine, speedloader, etc.).
    /// </summary>
    [JsonPropertyName("containerId")]
    public Guid ContainerId { get; set; }

    /// <summary>
    /// The character ID who owns the container.
    /// </summary>
    [JsonPropertyName("characterId")]
    public int CharacterId { get; set; }

    /// <summary>
    /// Number of rounds to unload from the container.
    /// </summary>
    [JsonPropertyName("roundsToUnload")]
    public int RoundsToUnload { get; set; }

    /// <summary>
    /// Type of ammo being unloaded (e.g., "9mm", "Arrow").
    /// </summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>
    /// Display name of the container for messages.
    /// </summary>
    [JsonPropertyName("containerName")]
    public string? ContainerName { get; set; }

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
    public static AmmoContainerUnloadPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AmmoContainerUnloadPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
