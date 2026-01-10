namespace Threa.Dal.Dto;

/// <summary>
/// Defines a school of magic with its properties.
/// GameMasters can add, edit, or remove magic schools.
/// </summary>
public class MagicSchoolDefinition
{
    /// <summary>
    /// Unique identifier for the school (e.g., "fire", "water", "shadow").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the magic school.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the school's nature and capabilities.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Short description for tooltips and lists.
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Hex color code for UI theming (e.g., "#FF4500" for fire).
    /// </summary>
    public string ColorCode { get; set; } = "#FFFFFF";

    /// <summary>
    /// Icon class or URL for visual representation.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Whether this school is active and available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a core school that cannot be deleted.
    /// Core schools: Fire, Water, Light, Life.
    /// </summary>
    public bool IsCore { get; set; } = false;

    /// <summary>
    /// The skill ID for channeling mana of this school (e.g., "fire-mana").
    /// </summary>
    public string ManaSkillId { get; set; } = string.Empty;

    /// <summary>
    /// Sort order for display purposes.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Typical spell types associated with this school (for documentation).
    /// </summary>
    public string? TypicalSpellTypes { get; set; }
}
