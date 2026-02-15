using System.Collections.Generic;

namespace GameMechanics.Combat;

/// <summary>
/// Request for multi-damage-type resolution.
/// A single attack can deal multiple damage types simultaneously
/// (e.g., a flaming sword: Cutting +4 AND Energy +2).
/// </summary>
public class MultiDamageRequest
{
  /// <summary>
  /// Base SV from the attack roll before weapon/ammo bonuses.
  /// Each damage type gets effectiveSV = baseSV + typeModifier.
  /// </summary>
  public int BaseSV { get; init; }

  /// <summary>
  /// Per-damage-type SV modifiers from weapon and ammo combined.
  /// </summary>
  public WeaponDamageProfile WeaponDamage { get; init; } = new();

  /// <summary>
  /// The damage class of the attack (1-4). Applies to all damage types from this weapon.
  /// </summary>
  public int DamageClass { get; init; } = 1;

  /// <summary>
  /// The hit location struck.
  /// </summary>
  public HitLocation HitLocation { get; init; }

  /// <summary>
  /// The defender's Armor Skill Ability Score.
  /// </summary>
  public int DefenderArmorAS { get; init; }

  /// <summary>
  /// Whether the shield block succeeded.
  /// </summary>
  public bool ShieldBlockSucceeded { get; init; }

  /// <summary>
  /// The shield block RV.
  /// </summary>
  public int? ShieldBlockRV { get; init; }

  /// <summary>
  /// The defender's shield info.
  /// </summary>
  public ShieldInfo? Shield { get; init; }

  /// <summary>
  /// Armor pieces equipped by the defender, ordered by layer.
  /// </summary>
  public List<ArmorInfo> ArmorPieces { get; init; } = new();
}
