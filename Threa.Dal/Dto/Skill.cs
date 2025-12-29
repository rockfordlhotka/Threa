namespace Threa.Dal.Dto;

/// <summary>
/// Skill definition with action system properties.
/// </summary>
public class Skill 
{
    /// <summary>
    /// Unique identifier for the skill.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Category grouping for the skill (Combat, Social, Crafting, etc.)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the skill.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a specialized skill requiring training.
    /// </summary>
    public bool IsSpecialized { get; set; }

    /// <summary>
    /// Whether this is a magic skill.
    /// </summary>
    public bool IsMagic { get; set; }

    /// <summary>
    /// Whether this is a theology/divine skill.
    /// </summary>
    public bool IsTheology { get; set; }

    /// <summary>
    /// Whether this is a psionic skill.
    /// </summary>
    public bool IsPsionic { get; set; }

    /// <summary>
    /// Untrained XP cost multiplier.
    /// </summary>
    public int Untrained { get; set; }

    /// <summary>
    /// Trained XP cost multiplier.
    /// </summary>
    public int Trained { get; set; }

    /// <summary>
    /// Primary attribute(s) for AS calculation.
    /// Multiple attributes separated by '/' are averaged.
    /// </summary>
    public string PrimaryAttribute { get; set; } = string.Empty;

    /// <summary>
    /// Secondary skill for cascading bonuses (if applicable).
    /// </summary>
    public string? SecondarySkill { get; set; }

    /// <summary>
    /// Tertiary skill for cascading bonuses (if applicable).
    /// </summary>
    public string? TertiarySkill { get; set; }

    /// <summary>
    /// URL to skill icon/image.
    /// </summary>
    public string? ImageUrl { get; set; }

    // === Action System Properties ===

    /// <summary>
    /// The type of action performed when this skill is used.
    /// </summary>
    public ActionType ActionType { get; set; } = ActionType.None;

    /// <summary>
    /// How Target Value is determined for this skill's actions.
    /// </summary>
    public TargetValueType TargetValueType { get; set; } = TargetValueType.Fixed;

    /// <summary>
    /// Default fixed TV for this skill (if TargetValueType is Fixed).
    /// Standard difficulties: Easy=4, Routine=6, Moderate=8, Challenging=10, Hard=12
    /// </summary>
    public int DefaultTV { get; set; } = 6;

    /// <summary>
    /// The skill ID used by opponents to oppose this action.
    /// Only applies when TargetValueType is Opposed.
    /// </summary>
    public string? OpposedSkillId { get; set; }

    /// <summary>
    /// How cooldowns work for this skill.
    /// </summary>
    public CooldownType CooldownType { get; set; } = CooldownType.None;

    /// <summary>
    /// Cooldown duration in seconds (for Fixed cooldown type).
    /// </summary>
    public int CooldownSeconds { get; set; } = 0;

    /// <summary>
    /// Which result table to use for interpreting SV.
    /// </summary>
    public ResultTableType ResultTable { get; set; } = ResultTableType.General;

    /// <summary>
    /// Whether this skill applies the Physicality damage bonus on success.
    /// Typically true for melee and thrown weapon attacks.
    /// </summary>
    public bool AppliesPhysicalityBonus { get; set; } = false;

    /// <summary>
    /// Whether this skill requires a target entity.
    /// </summary>
    public bool RequiresTarget { get; set; } = false;

    /// <summary>
    /// Whether this skill requires line of sight to the target.
    /// </summary>
    public bool RequiresLineOfSight { get; set; } = false;

    /// <summary>
    /// Mana cost for spells (0 for non-spell skills).
    /// </summary>
    public int ManaCost { get; set; } = 0;

    /// <summary>
    /// Whether this skill is a free action (no AP/FAT cost).
    /// </summary>
    public bool IsFreeAction { get; set; } = false;

    /// <summary>
    /// Whether this is a passive skill that activates automatically.
    /// Examples: armor absorption, automatic reactions.
    /// </summary>
    public bool IsPassive { get; set; } = false;

    /// <summary>
    /// Description of what this skill does as an action.
    /// </summary>
    public string? ActionDescription { get; set; }
}

