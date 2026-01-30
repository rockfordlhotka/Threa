using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for spell casting deferred action.
/// STUB: Full implementation pending spell system in future milestone.
/// </summary>
public class SpellCastPayload
{
    /// <summary>
    /// Spell ID to cast.
    /// </summary>
    [JsonPropertyName("spellId")]
    public int SpellId { get; set; }

    /// <summary>
    /// Target character ID (if single-target spell).
    /// </summary>
    [JsonPropertyName("targetId")]
    public Guid? TargetId { get; set; }

    /// <summary>
    /// Additional spell parameters (spell-specific).
    /// </summary>
    [JsonPropertyName("parameters")]
    public string? Parameters { get; set; }

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
    public static SpellCastPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SpellCastPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
