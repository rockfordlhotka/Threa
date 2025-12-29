namespace Threa.Dal.Dto;

/// <summary>
/// The type of entity an effect can target.
/// </summary>
public enum EffectTargetType
{
    /// <summary>
    /// Effect applies to a player character.
    /// </summary>
    Character = 0,

    /// <summary>
    /// Effect applies to an NPC (functionally same as character).
    /// </summary>
    Npc = 1,

    /// <summary>
    /// Effect applies to an item.
    /// </summary>
    Item = 2,

    /// <summary>
    /// Effect applies to a location (future use).
    /// </summary>
    Location = 3
}
