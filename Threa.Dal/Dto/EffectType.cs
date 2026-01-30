namespace Threa.Dal.Dto;

/// <summary>
/// Categories of effects that can be applied to entities.
/// </summary>
public enum EffectType
{
    /// <summary>
    /// Physical injury to a body location.
    /// </summary>
    Wound = 0,

    /// <summary>
    /// General status condition for neutral or mechanical states.
    /// For clearly beneficial narrative effects (Invisibility, Flying), use Buff instead.
    /// For clearly harmful narrative effects (Stunned, Paralyzed), use Debuff instead.
    /// </summary>
    Condition = 1,

    /// <summary>
    /// Poison damage and effects over time.
    /// </summary>
    Poison = 2,

    /// <summary>
    /// Disease effects and progression.
    /// </summary>
    Disease = 3,

    /// <summary>
    /// Positive temporary enhancement.
    /// </summary>
    Buff = 4,

    /// <summary>
    /// Negative temporary hindrance.
    /// </summary>
    Debuff = 5,

    /// <summary>
    /// Magical effect from a spell.
    /// </summary>
    SpellEffect = 6,

    /// <summary>
    /// Effect applied to an object (enchantment, curse, etc.).
    /// </summary>
    ObjectEffect = 7,

    /// <summary>
    /// Environmental effect (weather, terrain, magical zone).
    /// </summary>
    Environmental = 8,

    /// <summary>
    /// Effect from a magic or tech item (equipment, implants, etc.).
    /// </summary>
    ItemEffect = 9,

    /// <summary>
    /// Combat stance or mode (parry mode, defensive stance, etc.).
    /// Ends when character takes non-compatible action.
    /// </summary>
    CombatStance = 10,

    /// <summary>
    /// Concentration effect for long-running actions (reloading, rituals, etc.).
    /// Progress tracked per round, can be interrupted.
    /// </summary>
    Concentration = 11
}
