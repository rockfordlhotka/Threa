using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// State for concentration interruption penalty effects.
/// Used when post-use skill concentration is interrupted.
/// </summary>
public class ConcentrationInterruptionPenaltyState
{
    /// <summary>
    /// The skill ID that was interrupted.
    /// </summary>
    [JsonPropertyName("skillId")]
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// The skill name that was interrupted.
    /// </summary>
    [JsonPropertyName("skillName")]
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// Ability Score penalty amount (typically -1).
    /// </summary>
    [JsonPropertyName("penaltyAmount")]
    public int PenaltyAmount { get; set; }

    /// <summary>
    /// Rounds remaining for the penalty.
    /// </summary>
    [JsonPropertyName("roundsRemaining")]
    public int RoundsRemaining { get; set; }
}
