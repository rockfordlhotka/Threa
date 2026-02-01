using System;
using GameMechanics.Combat;

namespace GameMechanics.Messaging;

/// <summary>
/// Represents an active targeting interaction between an attacker and defender.
/// </summary>
public class TargetingInteraction
{
    /// <summary>
    /// Unique identifier for this interaction.
    /// </summary>
    public Guid InteractionId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The table where this interaction is happening.
    /// </summary>
    public Guid TableId { get; init; }

    /// <summary>
    /// ID of the attacking character.
    /// </summary>
    public int AttackerId { get; init; }

    /// <summary>
    /// Name of the attacking character.
    /// </summary>
    public string AttackerName { get; init; } = string.Empty;

    /// <summary>
    /// ID of the defending character.
    /// </summary>
    public int DefenderId { get; init; }

    /// <summary>
    /// Name of the defending character.
    /// </summary>
    public string DefenderName { get; init; } = string.Empty;

    /// <summary>
    /// Current state of the interaction.
    /// </summary>
    public TargetingState State { get; set; } = TargetingState.Initiated;

    /// <summary>
    /// Attacker-provided data.
    /// </summary>
    public TargetingAttackerData AttackerData { get; set; } = new();

    /// <summary>
    /// Defender-provided data.
    /// </summary>
    public TargetingDefenderData? DefenderData { get; set; }

    /// <summary>
    /// Whether the attacker has confirmed their settings.
    /// </summary>
    public bool AttackerConfirmed { get; set; }

    /// <summary>
    /// Whether the defender has confirmed their settings.
    /// </summary>
    public bool DefenderConfirmed { get; set; }

    /// <summary>
    /// Resolution data after the attack is resolved.
    /// </summary>
    public TargetingResolutionData? Resolution { get; set; }

    /// <summary>
    /// When this interaction was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When this interaction was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
