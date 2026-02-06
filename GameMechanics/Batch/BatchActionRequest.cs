using System;
using System.Collections.Generic;

namespace GameMechanics.Batch;

/// <summary>
/// Request to apply a batch action (damage or healing) to multiple characters.
/// </summary>
public record BatchActionRequest
{
    /// <summary>
    /// The table where the characters are playing.
    /// </summary>
    public Guid TableId { get; set; }

    /// <summary>
    /// IDs of characters to apply the action to.
    /// </summary>
    public List<int> CharacterIds { get; set; } = new();

    /// <summary>
    /// The type of action to apply.
    /// </summary>
    public BatchActionType ActionType { get; set; }

    /// <summary>
    /// The health pool to target: "FAT" (Fatigue) or "VIT" (Vitality).
    /// </summary>
    public string Pool { get; set; } = "FAT";

    /// <summary>
    /// The amount of damage or healing to apply.
    /// </summary>
    public int Amount { get; set; }
}

/// <summary>
/// The type of batch action to apply.
/// </summary>
public enum BatchActionType
{
    /// <summary>Apply damage to the specified pool.</summary>
    Damage,

    /// <summary>Apply healing to the specified pool.</summary>
    Healing
}
