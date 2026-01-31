using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for medical healing deferred action.
/// Stores all data needed to complete healing when concentration finishes.
/// </summary>
public class MedicalHealingPayload
{
    /// <summary>
    /// The character ID receiving healing.
    /// </summary>
    [JsonPropertyName("targetCharacterId")]
    public int TargetCharacterId { get; set; }

    /// <summary>
    /// Display name of the target for messages.
    /// </summary>
    [JsonPropertyName("targetName")]
    public string TargetName { get; set; } = "";

    /// <summary>
    /// Success Value from the skill check (determines healing amount).
    /// </summary>
    [JsonPropertyName("successValue")]
    public int SuccessValue { get; set; }

    /// <summary>
    /// Name of the medical skill used (First-Aid, Nursing, Doctor).
    /// </summary>
    [JsonPropertyName("skillName")]
    public string SkillName { get; set; } = "";

    /// <summary>
    /// The character ID of the healer (for messages).
    /// </summary>
    [JsonPropertyName("healerCharacterId")]
    public int HealerCharacterId { get; set; }

    /// <summary>
    /// Display name of the healer for messages.
    /// </summary>
    [JsonPropertyName("healerName")]
    public string HealerName { get; set; } = "";

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
    public static MedicalHealingPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<MedicalHealingPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
