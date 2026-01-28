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
    /// The type of this effect (stored directly to avoid relying on Definition navigation property).
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// Display name for this effect instance (stored directly to avoid relying on Definition).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of this effect instance.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Description of what caused this effect (spell name, item, attacker, etc.).
    /// </summary>
    public string? Source { get; set; }

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
    /// OBSOLETE: Use epoch-based expiration for performance.
    /// </summary>
    public int? RoundsRemaining { get; set; }

    /// <summary>
    /// For damage-over-time: rounds until next damage application.
    /// </summary>
    public int? RoundsUntilTick { get; set; }

    /// <summary>
    /// Game time (in seconds from epoch 0) when this effect was created.
    /// Used for epoch-based expiration (preferred for performance with large time skips).
    /// </summary>
    public long? CreatedAtEpochSeconds { get; set; }

    /// <summary>
    /// Game time (in seconds from epoch 0) when this effect expires.
    /// Null for permanent/until-removed effects.
    /// Pre-calculated at creation time for O(1) expiration checks.
    /// </summary>
    public long? ExpiresAtEpochSeconds { get; set; }

    /// <summary>
    /// Entity that caused this effect (character, NPC, item, or null for environmental).
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// If this effect came from a specific item.
    /// </summary>
    public Guid? SourceItemId { get; set; }

    /// <summary>
    /// If this effect is from an item, specifies when the effect activates/deactivates.
    /// </summary>
    public ItemEffectTrigger ItemEffectTrigger { get; set; }

    /// <summary>
    /// Whether this effect is cursed and prevents removing the source item.
    /// </summary>
    public bool IsCursed { get; set; }

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
