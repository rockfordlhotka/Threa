namespace Threa.Dal.Dto;

/// <summary>
/// Represents a modifier to a character attribute provided by an item.
/// </summary>
public class ItemAttributeModifier
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The item template this modifier belongs to.
    /// </summary>
    public int ItemTemplateId { get; set; }

    /// <summary>
    /// The attribute that receives the modifier (STR, DEX, END, INT, ITT, WIL, PHY).
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// The type of modifier applied.
    /// </summary>
    public BonusType ModifierType { get; set; }

    /// <summary>
    /// The value of the modifier.
    /// </summary>
    public decimal ModifierValue { get; set; }

    /// <summary>
    /// Optional condition for when this modifier applies.
    /// </summary>
    public string? Condition { get; set; }
}
