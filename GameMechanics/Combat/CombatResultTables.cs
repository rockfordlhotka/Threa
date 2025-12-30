using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Types of combat result tables available.
  /// </summary>
  public enum CombatTableType
  {
    /// <summary>
    /// Physicality damage bonus table.
    /// Converts Physicality check RV to SV bonus or attacker debuff.
    /// </summary>
    PhysicalityBonus,

    /// <summary>
    /// Standard damage table.
    /// Converts SV to FAT/VIT damage.
    /// </summary>
    Damage
  }

  /// <summary>
  /// Result from a Physicality bonus lookup.
  /// </summary>
  public class PhysicalityBonusResult
  {
    /// <summary>
    /// Bonus (or penalty) to add to attack SV.
    /// </summary>
    public int SVModifier { get; init; }

    /// <summary>
    /// AV penalty applied to the attacker (negative RV results).
    /// </summary>
    public int AttackerAVPenalty { get; init; }

    /// <summary>
    /// Duration in rounds for the AV penalty.
    /// </summary>
    public int PenaltyDurationRounds { get; init; }

    /// <summary>
    /// Human-readable description of the result.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    public static PhysicalityBonusResult None => new()
    {
      SVModifier = 0,
      AttackerAVPenalty = 0,
      PenaltyDurationRounds = 0,
      Description = "No effect"
    };
  }

  /// <summary>
  /// Result from a damage lookup.
  /// </summary>
  public class DamageResult
  {
    /// <summary>
    /// Fatigue damage dealt.
    /// </summary>
    public int FatigueDamage { get; init; }

    /// <summary>
    /// Vitality damage dealt.
    /// </summary>
    public int VitalityDamage { get; init; }

    /// <summary>
    /// Whether this result causes a wound.
    /// </summary>
    public bool CausesWound { get; init; }

    /// <summary>
    /// Human-readable description of the result.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    public static DamageResult None => new()
    {
      FatigueDamage = 0,
      VitalityDamage = 0,
      CausesWound = false,
      Description = "No damage"
    };
  }

  /// <summary>
  /// Lookup tables for combat results.
  /// Provides a unified interface for various result-value tables.
  /// </summary>
  public static class CombatResultTables
  {
    /// <summary>
    /// Looks up the Physicality bonus result for a given RV.
    /// RV = (Physicality AS + 4dF+) - 8
    /// </summary>
    /// <param name="rv">The result value from the Physicality check.</param>
    /// <returns>The bonus/penalty result.</returns>
    public static PhysicalityBonusResult GetPhysicalityBonus(int rv)
    {
      return rv switch
      {
        <= -9 => new PhysicalityBonusResult
        {
          SVModifier = 0,
          AttackerAVPenalty = -3,
          PenaltyDurationRounds = 3,
          Description = "Overextended: -3 AV for 3 rounds"
        },
        -8 or -7 => new PhysicalityBonusResult
        {
          SVModifier = 0,
          AttackerAVPenalty = -2,
          PenaltyDurationRounds = 2,
          Description = "Off-balance: -2 AV for 2 rounds"
        },
        -6 or -5 => new PhysicalityBonusResult
        {
          SVModifier = 0,
          AttackerAVPenalty = -2,
          PenaltyDurationRounds = 1,
          Description = "Strained: -2 AV for 1 round"
        },
        -4 or -3 => new PhysicalityBonusResult
        {
          SVModifier = 0,
          AttackerAVPenalty = -1,
          PenaltyDurationRounds = 1,
          Description = "Weak swing: -1 AV for 1 round"
        },
        >= -2 and <= 1 => PhysicalityBonusResult.None,
        2 or 3 => new PhysicalityBonusResult
        {
          SVModifier = 1,
          AttackerAVPenalty = 0,
          PenaltyDurationRounds = 0,
          Description = "Solid hit: +1 SV"
        },
        >= 4 and <= 7 => new PhysicalityBonusResult
        {
          SVModifier = 2,
          AttackerAVPenalty = 0,
          PenaltyDurationRounds = 0,
          Description = "Powerful blow: +2 SV"
        },
        >= 8 and <= 11 => new PhysicalityBonusResult
        {
          SVModifier = 3,
          AttackerAVPenalty = 0,
          PenaltyDurationRounds = 0,
          Description = "Crushing strike: +3 SV"
        },
        >= 12 => new PhysicalityBonusResult
        {
          SVModifier = 4,
          AttackerAVPenalty = 0,
          PenaltyDurationRounds = 0,
          Description = "Devastating blow: +4 SV"
        }
      };
    }

    /// <summary>
    /// Looks up damage for a given SV using the standard damage table.
    /// </summary>
    /// <param name="sv">The success value after all modifiers.</param>
    /// <returns>The damage result.</returns>
    /// <remarks>
    /// This is a simplified damage table. The full table from
    /// GAME_RULES_SPECIFICATION.md has more granular values.
    /// </remarks>
    public static DamageResult GetDamage(int sv)
    {
      if (sv < 0)
        return DamageResult.None;

      return sv switch
      {
        0 => new DamageResult
        {
          FatigueDamage = 1,
          VitalityDamage = 0,
          CausesWound = false,
          Description = "Glancing blow: 1 FAT"
        },
        1 => new DamageResult
        {
          FatigueDamage = 2,
          VitalityDamage = 0,
          CausesWound = false,
          Description = "Light hit: 2 FAT"
        },
        2 => new DamageResult
        {
          FatigueDamage = 3,
          VitalityDamage = 0,
          CausesWound = false,
          Description = "Solid hit: 3 FAT"
        },
        3 => new DamageResult
        {
          FatigueDamage = 4,
          VitalityDamage = 0,
          CausesWound = false,
          Description = "Good hit: 4 FAT"
        },
        4 => new DamageResult
        {
          FatigueDamage = 5,
          VitalityDamage = 1,
          CausesWound = false,
          Description = "Strong hit: 5 FAT, 1 VIT"
        },
        5 => new DamageResult
        {
          FatigueDamage = 6,
          VitalityDamage = 2,
          CausesWound = false,
          Description = "Heavy hit: 6 FAT, 2 VIT"
        },
        6 => new DamageResult
        {
          FatigueDamage = 7,
          VitalityDamage = 3,
          CausesWound = true,
          Description = "Wounding blow: 7 FAT, 3 VIT, wound"
        },
        7 => new DamageResult
        {
          FatigueDamage = 8,
          VitalityDamage = 4,
          CausesWound = true,
          Description = "Serious wound: 8 FAT, 4 VIT, wound"
        },
        _ => new DamageResult
        {
          FatigueDamage = 8 + (sv - 7),
          VitalityDamage = 5 + (sv - 8),
          CausesWound = true,
          Description = $"Critical wound: {8 + (sv - 7)} FAT, {5 + (sv - 8)} VIT, wound"
        }
      };
    }
  }
}
