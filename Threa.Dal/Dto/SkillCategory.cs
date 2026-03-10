namespace Threa.Dal.Dto;

/// <summary>
/// Category classification for skills to help UI display appropriate controls.
/// </summary>
public enum SkillCategory
{
    /// <summary>
    /// Standard attribute-based skills (Physicality, Dodge, etc.)
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Combat skills like weapon proficiencies.
    /// </summary>
    Combat = 1,

    /// <summary>
    /// Movement and athletics skills.
    /// </summary>
    Movement = 2,

    /// <summary>
    /// Social interaction skills.
    /// </summary>
    Social = 3,

    /// <summary>
    /// Crafting and creation skills.
    /// </summary>
    Crafting = 4,

    /// <summary>
    /// Knowledge and lore skills.
    /// </summary>
    Knowledge = 5,

    /// <summary>
    /// Medical and healing skills (First-Aid, Nursing, Doctor, etc.)
    /// </summary>
    Medical = 6,

    /// <summary>
    /// Mana gathering/channeling skills (Fire Mana, Water Mana, etc.)
    /// </summary>
    Mana = 10,

    /// <summary>
    /// Active spell skills that consume mana.
    /// </summary>
    Spell = 11,

    /// <summary>
    /// Theology/divine magic skills.
    /// </summary>
    Theology = 12,

    /// <summary>
    /// Psionic/mental power skills.
    /// </summary>
    Psionic = 13,

    /// <summary>
    /// Other miscellaneous skills.
    /// </summary>
    Other = 99
}
