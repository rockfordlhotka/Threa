namespace Threa.Dal.Dto;

/// <summary>
/// Represents a bonus to a specific skill provided by an item.
/// </summary>
public class ItemSkillBonus
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The item template this bonus belongs to.
    /// </summary>
    public int ItemTemplateId { get; set; }

    /// <summary>
    /// The skill that receives the bonus.
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// The type of bonus applied.
    /// </summary>
    public BonusType BonusType { get; set; }

    /// <summary>
    /// The value of the bonus.
    /// </summary>
    public decimal BonusValue { get; set; }

    /// <summary>
    /// Optional condition for when this bonus applies (e.g., "daytime", "night").
    /// </summary>
    public string? Condition { get; set; }
}
