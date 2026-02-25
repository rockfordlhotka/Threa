namespace Threa.Dal.Dto;

/// <summary>
/// Types of bonuses that items can provide.
/// </summary>
public enum BonusType
{
    /// <summary>
    /// A flat value added to the base value.
    /// </summary>
    FlatBonus = 0,

    /// <summary>
    /// A percentage multiplier applied to the base value.
    /// </summary>
    PercentageBonus = 1,

    /// <summary>
    /// Reduces cooldown time for skills.
    /// </summary>
    CooldownReduction = 2,

    /// <summary>Grants the skill at the specified level (not additive; overrides if higher than native).</summary>
    GrantSkill = 3
}
