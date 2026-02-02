namespace Threa.Dal.Dto;

/// <summary>
/// Default disposition of an NPC template.
/// Determines initial attitude when instantiated from template.
/// </summary>
public enum NpcDisposition
{
    /// <summary>
    /// Hostile - default for enemies and combat encounters.
    /// </summary>
    Hostile = 0,

    /// <summary>
    /// Neutral - non-combatants, potential allies or enemies.
    /// </summary>
    Neutral = 1,

    /// <summary>
    /// Friendly - allies, helpers, quest givers.
    /// </summary>
    Friendly = 2
}
