using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Request for damage resolution after a successful attack.
  /// </summary>
  public class DamageRequest
  {
    /// <summary>
    /// The incoming Success Value from the attack.
    /// </summary>
    public int IncomingSV { get; init; }

    /// <summary>
    /// The type of damage being dealt.
    /// </summary>
    public DamageType DamageType { get; init; }

    /// <summary>
    /// The damage class of the attack (1-4).
    /// </summary>
    public int DamageClass { get; init; } = 1;

    /// <summary>
    /// The hit location struck.
    /// </summary>
    public HitLocation HitLocation { get; init; }

    /// <summary>
    /// The defender's Armor Skill Ability Score.
    /// Used for the free armor skill check.
    /// </summary>
    public int DefenderArmorAS { get; init; }

    /// <summary>
    /// Whether the shield block succeeded (from Phase 2 defense resolution).
    /// </summary>
    public bool ShieldBlockSucceeded { get; init; }

    /// <summary>
    /// The shield block RV (determines absorption bonus, if shield block succeeded).
    /// </summary>
    public int? ShieldBlockRV { get; init; }

    /// <summary>
    /// The defender's shield info (if equipped and block succeeded).
    /// </summary>
    public ShieldInfo? Shield { get; init; }

    /// <summary>
    /// Armor pieces equipped by the defender, ordered by layer (outer first).
    /// Only armor covering the hit location will be used.
    /// </summary>
    public List<ArmorInfo> ArmorPieces { get; init; } = new();

    /// <summary>
    /// Creates a damage request from an attack result.
    /// </summary>
    public static DamageRequest FromAttack(
      AttackResult attack,
      DamageType damageType,
      int damageClass,
      int defenderArmorAS,
      List<ArmorInfo> armorPieces,
      ShieldInfo? shield = null,
      bool shieldBlockSucceeded = false,
      int? shieldBlockRV = null)
    {
      return new DamageRequest
      {
        IncomingSV = attack.FinalSV,
        DamageType = damageType,
        DamageClass = damageClass,
        HitLocation = attack.HitLocation ?? HitLocation.Torso,
        DefenderArmorAS = defenderArmorAS,
        ArmorPieces = armorPieces,
        Shield = shield,
        ShieldBlockSucceeded = shieldBlockSucceeded,
        ShieldBlockRV = shieldBlockRV
      };
    }
  }
}
