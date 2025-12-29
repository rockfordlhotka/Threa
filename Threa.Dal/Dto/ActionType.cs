namespace Threa.Dal.Dto;

/// <summary>
/// The type of action that a skill performs when used.
/// Determines resolution flow and result tables.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Default/unspecified action type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Offensive action targeting another entity.
    /// Uses combat damage result tables.
    /// </summary>
    Attack = 1,

    /// <summary>
    /// Defensive action against an attack.
    /// Uses opposed roll vs attacker.
    /// </summary>
    Defense = 2,

    /// <summary>
    /// Magical spell casting.
    /// May have mana costs and special effects.
    /// </summary>
    Spell = 3,

    /// <summary>
    /// Social interaction skill.
    /// Typically opposed by Focus or Reasoning.
    /// </summary>
    Social = 4,

    /// <summary>
    /// Creating or modifying items.
    /// Result determines quality of output.
    /// </summary>
    Craft = 5,

    /// <summary>
    /// Detecting or noticing things.
    /// May be opposed by Stealth or fixed TV.
    /// </summary>
    Perception = 6,

    /// <summary>
    /// Recalling information or identifying things.
    /// Typically fixed TV based on obscurity.
    /// </summary>
    Knowledge = 7,

    /// <summary>
    /// Physical feats of strength, agility, or endurance.
    /// Typically fixed TV based on difficulty.
    /// </summary>
    Physical = 8,

    /// <summary>
    /// Movement-related actions.
    /// May not require a roll in normal circumstances.
    /// </summary>
    Movement = 9,

    /// <summary>
    /// Healing or medical skills.
    /// Result determines HP/condition restored.
    /// </summary>
    Healing = 10,

    /// <summary>
    /// Mental or psychic abilities.
    /// Similar to spells but uses different power source.
    /// </summary>
    Psionic = 11,

    /// <summary>
    /// Divine or religious abilities.
    /// Similar to spells but faith-based.
    /// </summary>
    Theology = 12,

    /// <summary>
    /// Hiding or moving unseen.
    /// Opposed by Perception.
    /// </summary>
    Stealth = 13,

    /// <summary>
    /// A utility action that enables other actions.
    /// Examples: Aim, Prep, Ready.
    /// </summary>
    Prep = 14
}
