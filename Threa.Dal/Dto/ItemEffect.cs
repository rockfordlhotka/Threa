using System;

namespace Threa.Dal.Dto;

/// <summary>
/// An active effect on an item (enchantment, curse, temporary magic, etc.).
/// Can be effects on items in inventory or equipped on characters.
/// </summary>
public class ItemEffect
{
    /// <summary>
    /// Unique identifier for this effect instance.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The item this effect is applied to.
    /// </summary>
    public Guid ItemId { get; set; }

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
    /// For round-based effects: remaining rounds.
    /// </summary>
    public int? RoundsRemaining { get; set; }

    /// <summary>
    /// Entity that caused this effect.
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// Whether this effect is currently active.
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

    /// <summary>
    /// Reference to the item (populated when loading).
    /// </summary>
    public CharacterItem? Item { get; set; }
}
