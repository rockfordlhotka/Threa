namespace Threa.Dal.Dto;

/// <summary>
/// Defines a spell's properties beyond its base skill definition.
/// Links a spell skill to its magic school, type, costs, and effects.
/// </summary>
public class SpellDefinition
{
    /// <summary>
    /// The skill ID for this spell (matches SkillDefinition.Id).
    /// </summary>
    public string SkillId { get; set; } = string.Empty;

    /// <summary>
    /// The magic school this spell belongs to.
    /// Determines which mana pool is used.
    /// </summary>
    public MagicSchool MagicSchool { get; set; }

    /// <summary>
    /// How this spell targets and affects entities.
    /// </summary>
    public SpellType SpellType { get; set; }

    /// <summary>
    /// Base mana cost to cast this spell.
    /// </summary>
    public int ManaCost { get; set; }

    /// <summary>
    /// Spell range category.
    /// 0 = Self, 1 = Touch, 2 = Short (same area), 3 = Long (adjacent area)
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// For area effect spells, the radius of effect.
    /// </summary>
    public int? AreaRadius { get; set; }

    /// <summary>
    /// The effect definition applied on successful cast (if any).
    /// </summary>
    public int? EffectDefinitionId { get; set; }

    /// <summary>
    /// Default duration in rounds for the spell's effect.
    /// </summary>
    public int? DefaultDuration { get; set; }

    /// <summary>
    /// How the target resists this spell.
    /// </summary>
    public SpellResistanceType ResistanceType { get; set; }

    /// <summary>
    /// For Fixed resistance, the base TV to resist.
    /// </summary>
    public int? FixedResistanceTV { get; set; }

    /// <summary>
    /// For Opposed resistance, the skill ID the target uses to resist.
    /// </summary>
    public string? OpposedResistanceSkillId { get; set; }

    /// <summary>
    /// Description of the spell's effect for display.
    /// </summary>
    public string? EffectDescription { get; set; }
}

/// <summary>
/// How a target resists a spell.
/// </summary>
public enum SpellResistanceType
{
    /// <summary>
    /// No resistance - spell automatically succeeds if cast succeeds.
    /// Used for self-buffs and beneficial spells.
    /// </summary>
    None = 0,

    /// <summary>
    /// Fixed TV resistance (like ranged combat).
    /// </summary>
    Fixed = 1,

    /// <summary>
    /// Target rolls an opposed skill check.
    /// </summary>
    Opposed = 2,

    /// <summary>
    /// Target uses Willpower attribute for resistance.
    /// </summary>
    Willpower = 3
}
