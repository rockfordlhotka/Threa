namespace GameMechanics.Combat
{
  /// <summary>
  /// Types of damage in combat.
  /// Armor absorption values are defined per damage type.
  /// </summary>
  public enum DamageType
  {
    /// <summary>
    /// Impact damage from blunt weapons (maces, clubs, fists).
    /// </summary>
    Bashing,

    /// <summary>
    /// Slashing damage from edged weapons (swords, axes).
    /// </summary>
    Cutting,

    /// <summary>
    /// Puncture damage from pointed weapons (daggers, spears).
    /// </summary>
    Piercing,

    /// <summary>
    /// Ranged projectile damage (arrows, bolts, bullets).
    /// </summary>
    Projectile,

    /// <summary>
    /// Magical or technological energy damage (fire, lightning, plasma).
    /// </summary>
    Energy
  }
}
