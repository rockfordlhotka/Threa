namespace Threa.Dal.Dto;

/// <summary>
/// Defines a mana requirement for activating a spell skill.
/// Spell skills can require multiple mana types with minimum amounts.
/// </summary>
public class SkillManaRequirement
{
    /// <summary>
    /// The skill ID this requirement belongs to.
    /// </summary>
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// The magic school/mana type required.
    /// </summary>
    public MagicSchool MagicSchool { get; set; }

    /// <summary>
    /// Minimum amount of this mana type required to activate the spell.
    /// </summary>
    public int MinimumMana { get; set; }
}
