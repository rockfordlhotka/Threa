using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for skill use deferred action.
/// Stores all data needed to complete the skill use when pre-use concentration finishes,
/// or to apply penalties when post-use concentration is interrupted.
/// </summary>
public class SkillUsePayload
{
    /// <summary>
    /// The skill ID being used.
    /// </summary>
    [JsonPropertyName("skillId")]
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// The skill name being used.
    /// </summary>
    [JsonPropertyName("skillName")]
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Duration of penalty effect in rounds if post-use concentration is interrupted.
    /// </summary>
    [JsonPropertyName("interruptionPenaltyRounds")]
    public int InterruptionPenaltyRounds { get; set; }

    /// <summary>
    /// Additional context data for skill execution (optional).
    /// Can store target information, boost amounts, etc.
    /// </summary>
    [JsonPropertyName("additionalData")]
    public string? AdditionalData { get; set; }

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
    public static SkillUsePayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<SkillUsePayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
