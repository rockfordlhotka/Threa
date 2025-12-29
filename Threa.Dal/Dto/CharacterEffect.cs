using System;

namespace Threa.Dal.Dto;

/// <summary>
/// An active instance of an effect on a character.
/// CharacterEffects are created from EffectDefinitions when the effect is applied.
/// </summary>
public class CharacterEffect
{
    /// <summary>
    /// Unique identifier for this effect instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The character this effect is applied to.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// The effect definition this instance is based on.
    /// </summary>
    public int EffectDefinitionId { get; set; }

    /// <summary>
    /// Current stack count (for stackable effects).
    /// </summary>
    public int CurrentStacks { get; set; } = 1;

    /// <summary>
    /// When this effect was applied.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this effect expires (null for permanent/until-removed).
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// For round-based effects in combat: remaining rounds.
    /// </summary>
    public int? RoundsRemaining { get; set; }

    /// <summary>
    /// For damage-over-time: rounds until next damage application.
    /// </summary>
    public int? RoundsUntilTick { get; set; }

    /// <summary>
    /// Entity that caused this effect (character, NPC, item, or null for environmental).
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// If this effect came from a specific item.
    /// </summary>
    public Guid? SourceItemId { get; set; }

    /// <summary>
    /// For wounds: the specific body location.
    /// </summary>
    public string? WoundLocation { get; set; }

    /// <summary>
    /// Whether this effect is currently active (can be paused).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Custom notes about this specific instance.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Additional properties stored as JSON.
    /// </summary>
    public string? CustomProperties { get; set; }

    /// <summary>
    /// Reference to the effect definition (populated when loading).
    /// </summary>
    public EffectDefinition? Definition { get; set; }
}
