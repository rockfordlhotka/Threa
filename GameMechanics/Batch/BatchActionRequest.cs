using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Batch;

/// <summary>
/// Request to apply a batch action (damage, healing, visibility, dismiss, or effect add/remove)
/// to multiple characters.
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

    /// <summary>
    /// For Visibility actions: true = reveal, false = hide.
    /// Determined by SelectionBar before calling service (reveal when any hidden, hide when all visible).
    /// Null for non-visibility actions.
    /// </summary>
    public bool? VisibilityTarget { get; set; }

    /// <summary>
    /// For EffectAdd: the effect name to apply.
    /// </summary>
    public string? EffectName { get; set; }

    /// <summary>
    /// For EffectAdd: the effect type.
    /// </summary>
    public EffectType? EffectType { get; set; }

    /// <summary>
    /// For EffectAdd: the effect description.
    /// </summary>
    public string? EffectDescription { get; set; }

    /// <summary>
    /// For EffectAdd: duration in seconds (null = permanent).
    /// </summary>
    public long? DurationSeconds { get; set; }

    /// <summary>
    /// For EffectAdd: duration in rounds for legacy EffectRecord creation.
    /// </summary>
    public int? DurationRounds { get; set; }

    /// <summary>
    /// For EffectAdd: serialized EffectState JSON (modifiers, DoT, etc.).
    /// </summary>
    public string? BehaviorStateJson { get; set; }

    /// <summary>
    /// For EffectRemove: list of effect names to remove.
    /// </summary>
    public List<string> EffectNamesToRemove { get; set; } = new();
}

/// <summary>
/// The type of batch action to apply.
/// </summary>
public enum BatchActionType
{
    /// <summary>Apply damage to the specified pool.</summary>
    Damage,

    /// <summary>Apply healing to the specified pool.</summary>
    Healing,

    /// <summary>Toggle NPC visibility (reveal or hide).</summary>
    Visibility,

    /// <summary>Dismiss/archive NPCs and remove from table.</summary>
    Dismiss,

    /// <summary>Add an effect to selected characters.</summary>
    EffectAdd,

    /// <summary>Remove effect(s) from selected characters.</summary>
    EffectRemove
}
