using System;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents a narrative location where environmental spells can be placed.
/// Locations are simple named constructs for tracking spell effects in a TTRPG context.
/// </summary>
public class SpellLocation
{
    /// <summary>
    /// Unique identifier for this location instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The narrative name of the location (e.g., "The doorway", "Center of the room").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the location.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The campaign or session this location belongs to.
    /// </summary>
    public int? CampaignId { get; set; }

    /// <summary>
    /// When the location was created (for cleanup).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// An effect attached to a location (for environmental spells).
/// </summary>
public class LocationEffect
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The location this effect is attached to.
    /// </summary>
    public Guid LocationId { get; set; }

    /// <summary>
    /// The spell that created this effect.
    /// </summary>
    public string SpellSkillId { get; set; } = string.Empty;

    /// <summary>
    /// The caster who created this effect.
    /// </summary>
    public int CasterId { get; set; }

    /// <summary>
    /// The effect definition for what this does.
    /// </summary>
    public int? EffectDefinitionId { get; set; }

    /// <summary>
    /// Rounds remaining for this effect.
    /// </summary>
    public int? RoundsRemaining { get; set; }

    /// <summary>
    /// When this effect was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this effect expires (if time-based rather than round-based).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// The SV achieved when casting (for scaling effects).
    /// </summary>
    public int CastSV { get; set; }

    /// <summary>
    /// Description of the effect for narrative display.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this effect is still active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
