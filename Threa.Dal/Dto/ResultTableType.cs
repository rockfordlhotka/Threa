namespace Threa.Dal.Dto;

/// <summary>
/// Identifies which result table to use for interpreting
/// the Success Value (SV) of an action.
/// </summary>
public enum ResultTableType
{
    /// <summary>
    /// General success/failure table.
    /// Maps SV to quality descriptions (Marginal, Standard, Good, Excellent, Outstanding).
    /// </summary>
    General = 0,

    /// <summary>
    /// Combat damage table.
    /// Maps SV to damage dice rolls.
    /// </summary>
    CombatDamage = 1,

    /// <summary>
    /// Crafting quality table.
    /// Maps SV to item quality modifiers.
    /// </summary>
    Crafting = 2,

    /// <summary>
    /// Healing effectiveness table.
    /// Maps SV to HP/FAT restored.
    /// </summary>
    Healing = 3,

    /// <summary>
    /// Social influence table.
    /// Maps SV to degree of influence achieved.
    /// </summary>
    Social = 4,

    /// <summary>
    /// Perception information table.
    /// Maps SV to amount/quality of information gained.
    /// </summary>
    Perception = 5,

    /// <summary>
    /// Defense result table.
    /// Success means attack is avoided/blocked.
    /// </summary>
    Defense = 6,

    /// <summary>
    /// Spell effect table.
    /// Maps SV to spell effectiveness.
    /// </summary>
    SpellEffect = 7,

    /// <summary>
    /// Movement result table.
    /// Maps SV to distance achieved (in range values).
    /// </summary>
    Movement = 8
}
