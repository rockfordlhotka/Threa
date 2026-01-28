using System;

namespace Threa.Dal.Dto;

/// <summary>
/// Template for creating effects - provides defaults that GM can modify before applying.
/// Used for system-wide effect library (Stunned, Blessed, Poisoned, etc.).
/// </summary>
public class EffectTemplateDto
{
    /// <summary>
    /// Primary key (0 for new templates).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Template name (required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Effect category (Buff, Debuff, Condition, Poison, etc.).
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// What this effect does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon identifier for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Display color (hex code or CSS class name).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Default duration amount.
    /// </summary>
    public int? DefaultDurationValue { get; set; }

    /// <summary>
    /// Duration type (Rounds, Minutes, Hours, Days, Permanent).
    /// </summary>
    public DurationType DurationType { get; set; }

    /// <summary>
    /// Serialized EffectState with modifiers (JSON).
    /// </summary>
    public string? StateJson { get; set; }

    /// <summary>
    /// Comma-separated tags for organization/filtering.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// True for built-in templates that cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Soft delete flag (false = deleted).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
