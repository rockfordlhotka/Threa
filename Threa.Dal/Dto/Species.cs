using System.Collections.Generic;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents a playable species in the game.
/// </summary>
public class Species
{
    /// <summary>
    /// Unique identifier for the species.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the species.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the species.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Attribute modifiers for this species.
    /// </summary>
    public List<SpeciesAttributeModifier> AttributeModifiers { get; set; } = [];
}

/// <summary>
/// Represents a species-specific modifier to an attribute.
/// Applied during character creation: Attribute = 4dF + 10 + Modifier
/// </summary>
public class SpeciesAttributeModifier
{
    /// <summary>
    /// The attribute name (STR, DEX, END, INT, ITT, WIL, PHY).
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// The modifier value (positive or negative).
    /// </summary>
    public int Modifier { get; set; }
}
