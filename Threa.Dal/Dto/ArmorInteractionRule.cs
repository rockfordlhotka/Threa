namespace Threa.Dal.Dto;

/// <summary>
/// Controls how a weapon's on-hit target effect interacts with the target's armor and shield.
/// </summary>
public enum ArmorInteractionRule
{
    /// <summary>
    /// Effect only applies if the attack penetrated the target's armor (default).
    /// If there is no armor at the hit location, the effect always applies.
    /// </summary>
    PenetrationOnly = 0,

    /// <summary>
    /// Effect only applies if the target has no armor at the hit location.
    /// </summary>
    NoArmor = 1,

    /// <summary>
    /// Effect ignores armor completely — applies even if armor absorbed the hit.
    /// </summary>
    IgnoreArmor = 2
}
