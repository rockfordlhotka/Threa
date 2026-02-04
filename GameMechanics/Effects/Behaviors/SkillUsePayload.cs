using System;
using System.Collections.Generic;
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
    /// Deprecated: Use InterruptionDebuff instead for full configuration.
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
    /// Effects on targets that are maintained while concentrating.
    /// When concentration ends (any reason), these effects are removed.
    /// </summary>
    [JsonPropertyName("linkedEffects")]
    public List<LinkedEffectInfo>? LinkedEffects { get; set; }

    /// <summary>
    /// Configuration for the debuff applied when concentration is interrupted.
    /// If null, falls back to InterruptionPenaltyRounds with default -1 AS penalty.
    /// </summary>
    [JsonPropertyName("interruptionDebuff")]
    public InterruptionDebuffConfig? InterruptionDebuff { get; set; }

    /// <summary>
    /// Serializes this payload to JSON for storage in ConcentrationState.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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

/// <summary>
/// Information about an effect on a target that is linked to the caster's concentration.
/// When the caster's concentration ends, this effect should be removed from the target.
/// </summary>
public class LinkedEffectInfo
{
    /// <summary>
    /// The effect ID on the target character.
    /// </summary>
    [JsonPropertyName("effectId")]
    public Guid EffectId { get; set; }

    /// <summary>
    /// The character ID who has this effect.
    /// </summary>
    [JsonPropertyName("targetCharacterId")]
    public int TargetCharacterId { get; set; }

    /// <summary>
    /// Display description for logging (e.g., "Blinded on Goblin").
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Configuration for the debuff applied when concentration is interrupted.
/// Allows GM-configurable penalties beyond the default -1 AS.
/// </summary>
public class InterruptionDebuffConfig
{
    /// <summary>
    /// Name of the debuff effect (e.g., "Blindness Concentration Broken").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Concentration Broken";

    /// <summary>
    /// Description for the debuff effect shown to the player.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Duration of the debuff in rounds.
    /// </summary>
    [JsonPropertyName("durationRounds")]
    public int DurationRounds { get; set; } = 3;

    /// <summary>
    /// Global AS penalty applied to all skills. Default: -1
    /// </summary>
    [JsonPropertyName("globalAsPenalty")]
    public int GlobalAsPenalty { get; set; } = -1;

    /// <summary>
    /// Optional: Specific skill penalties (skill ID -> penalty value).
    /// These are in addition to the global penalty.
    /// </summary>
    [JsonPropertyName("skillPenalties")]
    public Dictionary<string, int>? SkillPenalties { get; set; }

    /// <summary>
    /// Optional: Attribute penalties (attribute name -> penalty value).
    /// </summary>
    [JsonPropertyName("attributePenalties")]
    public Dictionary<string, int>? AttributePenalties { get; set; }

    /// <summary>
    /// Serializes this config to JSON.
    /// </summary>
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Deserializes a config from JSON.
    /// </summary>
    public static InterruptionDebuffConfig? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<InterruptionDebuffConfig>(json);
        }
        catch
        {
            return null;
        }
    }
}
