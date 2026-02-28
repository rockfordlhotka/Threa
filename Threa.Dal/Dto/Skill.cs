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
    /// Category classification for UI filtering and display logic.
    /// Determines which controls and properties are shown in the UI.
    /// </summary>
    public SkillCategory Category { get; set; } = SkillCategory.Standard;

    /// <summary>
    /// Display name of the skill.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the skill does.
    /// </summary>
    public string? Description { get; set; }

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
    /// Secondary attribute for cascading bonuses (if applicable).
    /// </summary>
    public string? SecondaryAttribute { get; set; }

    /// <summary>
    /// Tertiary attribute for cascading bonuses (if applicable).
    /// </summary>
    public string? TertiaryAttribute { get; set; }

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

    // === Spell-Specific Properties ===

    /// <summary>
    /// Whether this is a spell skill.
    /// </summary>
    public bool IsSpell => IsMagic || IsTheology || IsPsionic;

    /// <summary>
    /// Whether this is a mana channeling/gathering skill.
    /// UI should show mana-related controls.
    /// </summary>
    public bool IsManaSkill => Category == SkillCategory.Mana;

    /// <summary>
    /// Whether this is an active spell that requires mana and casting.
    /// UI should show spell-specific controls (mana requirements, pumping options).
    /// </summary>
    public bool IsActiveSpell => Category == SkillCategory.Spell || Category == SkillCategory.Theology || Category == SkillCategory.Psionic;

    /// <summary>
    /// Whether this is a combat skill.
    /// UI should show combat-related controls (damage, weapon properties).
    /// </summary>
    public bool IsCombatSkill => Category == SkillCategory.Combat;

    /// <summary>
    /// Bonus added to the Success Value after a successful check.
    /// Used by advanced medical skills (Nursing, Doctor) to represent greater expertise.
    /// </summary>
    public int SvBonus { get; set; } = 0;

    /// <summary>
    /// Whether this is a medical/healing skill.
    /// UI should show medical-specific controls (concentration rounds, healing result).
    /// </summary>
    public bool IsMedicalSkill => Category == SkillCategory.Medical;

    /// <summary>
    /// Whether this skill can be deleted.
    /// Standard attribute skills cannot be deleted as they are core to the game system.
    /// </summary>
    public bool CanBeDeleted => Category != SkillCategory.Standard;

    /// <summary>
    /// Mana requirements for activating this spell (for spell skills only).
    /// A spell can require multiple mana types with minimum amounts.
    /// </summary>
    public List<SkillManaRequirement>? ManaRequirements { get; set; }

    /// <summary>
    /// Whether this spell can be pumped with additional FAT to boost its effect.
    /// </summary>
    public bool CanPumpWithFatigue { get; set; } = false;

    /// <summary>
    /// Whether this spell can be pumped with any color of mana to boost its effect.
    /// </summary>
    public bool CanPumpWithMana { get; set; } = false;

    /// <summary>
    /// Description of what pumping does to this spell's effect.
    /// Example: "Each additional mana increases damage by 1 class"
    /// </summary>
    public string? PumpDescription { get; set; }

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

    // === Concentration Properties ===

    /// <summary>
    /// Whether this skill requires concentration before use (casting time).
    /// If true, the character must concentrate for the specified duration before the skill takes effect.
    /// If concentration is interrupted, the skill does not execute.
    /// </summary>
    public bool RequiresPreUseConcentration { get; set; } = false;

    /// <summary>
    /// Duration of pre-use concentration in rounds.
    /// Only applies when RequiresPreUseConcentration is true.
    /// </summary>
    public int PreUseConcentrationRounds { get; set; } = 0;

    /// <summary>
    /// Whether this skill requires concentration after use (cooldown-like effect).
    /// If true, the character must maintain concentration after using the skill.
    /// If concentration is interrupted, a penalty is applied.
    /// </summary>
    public bool RequiresPostUseConcentration { get; set; } = false;

    /// <summary>
    /// Duration of post-use concentration in rounds.
    /// Only applies when RequiresPostUseConcentration is true.
    /// </summary>
    public int PostUseConcentrationRounds { get; set; } = 0;

    /// <summary>
    /// Duration of penalty effect in rounds if post-use concentration is interrupted.
    /// Applies a -1 AS penalty for this many rounds.
    /// Only applies when RequiresPostUseConcentration is true.
    /// </summary>
    public int PostUseInterruptionPenaltyRounds { get; set; } = 0;
}

