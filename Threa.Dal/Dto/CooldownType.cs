namespace Threa.Dal.Dto;

/// <summary>
/// Defines how cooldowns work for actions.
/// </summary>
public enum CooldownType
{
    /// <summary>
    /// No cooldown - can be used again immediately.
    /// </summary>
    None = 0,

    /// <summary>
    /// Cooldown duration is based on skill level.
    /// Higher skill = shorter cooldown.
    /// Used for ranged weapons.
    /// </summary>
    SkillBased = 1,

    /// <summary>
    /// Fixed cooldown duration regardless of skill.
    /// Value is specified in seconds or rounds.
    /// </summary>
    Fixed = 2,

    /// <summary>
    /// Requires a prep action before use.
    /// Examples: Loading ammo, readying components.
    /// </summary>
    PrepRequired = 3
}
