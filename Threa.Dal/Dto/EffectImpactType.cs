namespace Threa.Dal.Dto;

/// <summary>
/// The type of impact an effect has on its target.
/// </summary>
public enum EffectImpactType
{
    /// <summary>
    /// Modifies a specific attribute (STR, DEX, etc.).
    /// </summary>
    AttributeModifier = 0,

    /// <summary>
    /// Modifies a specific skill or skill category.
    /// </summary>
    SkillModifier = 1,

    /// <summary>
    /// Modifies Ability Score for actions.
    /// </summary>
    ASModifier = 2,

    /// <summary>
    /// Modifies Attack Value.
    /// </summary>
    AVModifier = 3,

    /// <summary>
    /// Modifies Target Value (defense).
    /// </summary>
    TVModifier = 4,

    /// <summary>
    /// Modifies Success Value (damage/effect magnitude).
    /// </summary>
    SVModifier = 5,

    /// <summary>
    /// Modifies recovery rates (FAT/VIT/AP).
    /// </summary>
    RecoveryModifier = 6,

    /// <summary>
    /// Deals periodic damage to FAT or VIT.
    /// </summary>
    DamageOverTime = 7,

    /// <summary>
    /// Modifies movement range or speed.
    /// </summary>
    MovementModifier = 8,

    /// <summary>
    /// Grants or removes special abilities.
    /// </summary>
    SpecialAbility = 9
}
