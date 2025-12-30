namespace GameMechanics.Combat
{
  /// <summary>
  /// Types of defense available in combat.
  /// </summary>
  public enum DefenseType
  {
    /// <summary>
    /// No active defense. TV = Dodge AS - 1 (no roll, no cost).
    /// Used when defender has no AP, is surprised, or chooses not to defend.
    /// </summary>
    Passive,

    /// <summary>
    /// Active dodge. TV = Dodge AS + 4dF+ (costs 1 AP + 1 FAT or 2 AP).
    /// Works against melee and ranged attacks.
    /// </summary>
    Dodge,

    /// <summary>
    /// Active parry with weapon or shield. TV = Weapon/Shield AS + 4dF+.
    /// Requires entering parry mode first (costs 1 AP + 1 FAT or 2 AP).
    /// While in parry mode, parry defenses are free.
    /// Only works against melee attacks.
    /// </summary>
    Parry,

    /// <summary>
    /// Shield block. Rolls Shield AS + 4dF+ vs TV 8.
    /// On success, shield absorbs damage (handled in Phase 3).
    /// This is a "free action" like Physicality bonus.
    /// </summary>
    ShieldBlock
  }
}
