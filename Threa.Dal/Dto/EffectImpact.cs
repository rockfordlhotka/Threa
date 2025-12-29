namespace Threa.Dal.Dto;

/// <summary>
/// Represents a single impact/modifier that an effect applies.
/// An effect can have multiple impacts (e.g., a poison might deal damage AND apply a skill penalty).
/// </summary>
public class EffectImpact
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The effect definition this impact belongs to.
    /// </summary>
    public int EffectDefinitionId { get; set; }

    /// <summary>
    /// The type of impact (attribute modifier, skill modifier, damage, etc.).
    /// </summary>
    public EffectImpactType ImpactType { get; set; }

    /// <summary>
    /// Target of the impact (attribute name, skill name, "All", "Physical", "Mental", etc.).
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// The modifier/damage value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Whether the value is a percentage (true) or flat modifier (false).
    /// </summary>
    public bool IsPercentage { get; set; }

    /// <summary>
    /// For DamageOverTime: interval in rounds between damage applications.
    /// </summary>
    public int? DamageInterval { get; set; }

    /// <summary>
    /// Optional condition for when this impact applies.
    /// </summary>
    public string? Condition { get; set; }
}
