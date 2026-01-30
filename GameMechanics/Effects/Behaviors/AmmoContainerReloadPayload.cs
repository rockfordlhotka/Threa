using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for ammo container reload deferred action.
/// Stores all data needed to complete loading individual rounds into a container when concentration finishes.
/// </summary>
public class AmmoContainerReloadPayload
{
    /// <summary>
    /// The ammo container CharacterItem ID being loaded (magazine, speedloader, etc.).
    /// </summary>
    [JsonPropertyName("containerId")]
    public Guid ContainerId { get; set; }

    /// <summary>
    /// The loose ammo source CharacterItem ID.
    /// </summary>
    [JsonPropertyName("sourceItemId")]
    public Guid SourceItemId { get; set; }

    /// <summary>
    /// Number of rounds to load into the container.
    /// </summary>
    [JsonPropertyName("roundsToLoad")]
    public int RoundsToLoad { get; set; }

    /// <summary>
    /// Type of ammo being loaded (e.g., "9mm", "Arrow").
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
    public static AmmoContainerReloadPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AmmoContainerReloadPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
