namespace Threa.Dal.Dto;

/// <summary>
/// Categorizes spells by how they target and affect entities.
/// </summary>
public enum SpellType
{
    /// <summary>
    /// Affects a specific target (character, NPC, or object).
    /// Examples: Fire Bolt, Heal, Curse
    /// </summary>
    Targeted = 1,

    /// <summary>
    /// Enhances the caster only.
    /// Examples: Strength, Invisibility, Shield
    /// </summary>
    SelfBuff = 2,

    /// <summary>
    /// Affects multiple targets in an area.
    /// Examples: Fireball, Mass Heal, Fear
    /// </summary>
    AreaEffect = 3,

    /// <summary>
    /// Creates persistent effects at a location.
    /// Examples: Wall of Fire, Fog Cloud, Silence
    /// </summary>
    Environmental = 4
}
