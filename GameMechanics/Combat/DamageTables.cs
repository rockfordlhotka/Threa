using System;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Represents a dice roll specification for damage.
  /// </summary>
  public class DiceSpec
  {
    /// <summary>
    /// Number of dice to roll.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Number of sides on each die.
    /// </summary>
    public int Sides { get; init; }

    /// <summary>
    /// Divisor to apply to the roll result (for SV 0-1).
    /// </summary>
    public int Divisor { get; init; } = 1;

    /// <summary>
    /// Whether this roll escalates to the next damage class.
    /// </summary>
    public bool ClassEscalation { get; init; }

    /// <summary>
    /// Human-readable description of the dice roll.
    /// </summary>
    public string Description
    {
      get
      {
        var dice = $"{Count}d{Sides}";
        if (Divisor > 1)
          dice += $"/{Divisor}";
        if (ClassEscalation)
          dice += " (Class+1)";
        return dice;
      }
    }
  }

  /// <summary>
  /// Result of rolling damage dice and converting to FAT/VIT/Wounds.
  /// </summary>
  public class DamageRollResult
  {
    /// <summary>
    /// The dice specification used.
    /// </summary>
    public DiceSpec DiceSpec { get; init; } = new();

    /// <summary>
    /// Raw dice roll total before any modifiers.
    /// </summary>
    public int RawRoll { get; init; }

    /// <summary>
    /// Final damage value after divisor (minimum 1).
    /// </summary>
    public int DamageValue { get; init; }

    /// <summary>
    /// Fatigue damage to apply.
    /// </summary>
    public int FatigueDamage { get; init; }

    /// <summary>
    /// Vitality damage to apply.
    /// </summary>
    public int VitalityDamage { get; init; }

    /// <summary>
    /// Number of wounds inflicted.
    /// </summary>
    public int Wounds { get; init; }

    /// <summary>
    /// Whether damage class escalated (for extreme damage).
    /// </summary>
    public bool ClassEscalated { get; init; }

    /// <summary>
    /// Human-readable summary.
    /// </summary>
    public string Summary { get; init; } = string.Empty;
  }

  /// <summary>
  /// Lookup tables for converting SV to dice rolls and damage to FAT/VIT/Wounds.
  /// </summary>
  public static class DamageTables
  {
    /// <summary>
    /// Gets the dice specification for a given Success Value.
    /// </summary>
    /// <param name="sv">The success value (0+).</param>
    /// <returns>The dice to roll for damage.</returns>
    public static DiceSpec GetDiceForSV(int sv)
    {
      if (sv < 0) return new DiceSpec { Count = 0, Sides = 0 };

      return sv switch
      {
        0 => new DiceSpec { Count = 1, Sides = 6, Divisor = 3 },  // 1d6/3 = 1-2
        1 => new DiceSpec { Count = 1, Sides = 6, Divisor = 2 },  // 1d6/2 = 1-3
        2 => new DiceSpec { Count = 1, Sides = 6 },               // 1d6
        3 => new DiceSpec { Count = 1, Sides = 8 },               // 1d8
        4 => new DiceSpec { Count = 1, Sides = 10 },              // 1d10
        5 => new DiceSpec { Count = 1, Sides = 12 },              // 1d12
        6 => new DiceSpec { Count = 2, Sides = 7 },               // 1d6+1d8 approximated as 2d7 (avg 7)
        7 => new DiceSpec { Count = 2, Sides = 8 },               // 2d8
        8 => new DiceSpec { Count = 2, Sides = 10 },              // 2d10
        9 => new DiceSpec { Count = 2, Sides = 12 },              // 2d12
        10 => new DiceSpec { Count = 3, Sides = 10 },             // 3d10
        11 => new DiceSpec { Count = 3, Sides = 12 },             // 3d12
        12 or 13 or 14 => new DiceSpec { Count = 4, Sides = 10 }, // 4d10
        15 or 16 => new DiceSpec { Count = 1, Sides = 6, ClassEscalation = true },  // 1d6 Class+1
        17 or 18 => new DiceSpec { Count = 1, Sides = 8, ClassEscalation = true },  // 1d8 Class+1
        _ => new DiceSpec { Count = 1, Sides = 10, ClassEscalation = true }         // 1d10 Class+1 (SV 19+)
      };
    }

    /// <summary>
    /// Rolls damage dice using the provided dice roller.
    /// </summary>
    /// <param name="diceRoller">The dice roller to use.</param>
    /// <param name="sv">The success value.</param>
    /// <param name="damageClass">The weapon's damage class (1-4).</param>
    /// <returns>Complete damage roll result.</returns>
    public static DamageRollResult RollDamage(IDiceRoller diceRoller, int sv, int damageClass = 1)
    {
      if (sv < 0)
      {
        return new DamageRollResult
        {
          DiceSpec = new DiceSpec { Count = 0, Sides = 0 },
          RawRoll = 0,
          DamageValue = 0,
          FatigueDamage = 0,
          VitalityDamage = 0,
          Wounds = 0,
          Summary = "No damage (negative SV)"
        };
      }

      var diceSpec = GetDiceForSV(sv);

      // Roll the dice
      int rawRoll = 0;

      // Special handling for SV 6 (1d6 + 1d8)
      if (sv == 6)
      {
        rawRoll = diceRoller.Roll(1, 6) + diceRoller.Roll(1, 8);
      }
      else if (diceSpec.Count > 0 && diceSpec.Sides > 0)
      {
        rawRoll = diceRoller.Roll(diceSpec.Count, diceSpec.Sides);
      }

      // Apply divisor (minimum 1 damage)
      int damageValue = diceSpec.Divisor > 1
        ? Math.Max(1, rawRoll / diceSpec.Divisor)
        : rawRoll;

      // Handle class escalation
      bool classEscalated = diceSpec.ClassEscalation;
      if (classEscalated)
      {
        // At Class+1, damage is multiplied by 10
        damageValue *= 10;
      }

      // Convert damage to FAT/VIT/Wounds
      var conversion = ConvertDamage(damageValue);

      return new DamageRollResult
      {
        DiceSpec = diceSpec,
        RawRoll = rawRoll,
        DamageValue = damageValue,
        FatigueDamage = conversion.fatigue,
        VitalityDamage = conversion.vitality,
        Wounds = conversion.wounds,
        ClassEscalated = classEscalated,
        Summary = BuildSummary(diceSpec, rawRoll, damageValue, conversion, classEscalated)
      };
    }

    /// <summary>
    /// Converts a damage total to FAT/VIT/Wounds.
    /// </summary>
    /// <param name="damage">The damage total from dice roll.</param>
    /// <returns>Tuple of (fatigue, vitality, wounds).</returns>
    public static (int fatigue, int vitality, int wounds) ConvertDamage(int damage)
    {
      if (damage <= 0) return (0, 0, 0);

      // Damage 1-4: FAT only
      if (damage <= 4) return (damage, 0, 0);

      // Damage 5-6: FAT + light VIT
      if (damage == 5) return (5, 1, 0);
      if (damage == 6) return (6, 2, 0);

      // Damage 7-9: FAT + VIT + 1 wound
      if (damage == 7) return (7, 4, 1);
      if (damage == 8) return (8, 6, 1);
      if (damage == 9) return (9, 8, 1);

      // Damage 10-14: FAT + VIT + 2 wounds
      if (damage >= 10 && damage <= 14) return (damage, damage, 2);

      // Damage 15-19: FAT + VIT + 3 wounds
      if (damage >= 15 && damage <= 19) return (damage, damage, 3);

      // Damage 20+: FAT + VIT + scaled wounds
      // Every 10 damage = 2 more wounds (4 at 20, 6 at 30, etc.)
      int wounds = 4 + ((damage - 20) / 10) * 2;
      return (damage, damage, wounds);
    }

    /// <summary>
    /// Gets the conversion result for a specific damage value (for display purposes).
    /// </summary>
    public static DamageConversionResult GetConversionResult(int damage)
    {
      var (fatigue, vitality, wounds) = ConvertDamage(damage);
      return new DamageConversionResult
      {
        Damage = damage,
        FatigueDamage = fatigue,
        VitalityDamage = vitality,
        Wounds = wounds
      };
    }

    private static string BuildSummary(DiceSpec spec, int rawRoll, int damageValue,
      (int fatigue, int vitality, int wounds) conversion, bool classEscalated)
    {
      var parts = new System.Collections.Generic.List<string>();

      parts.Add($"Rolled {spec.Description}: {rawRoll}");

      if (spec.Divisor > 1)
        parts.Add($"/{spec.Divisor} = {damageValue}");

      if (classEscalated)
        parts.Add($"x10 (class escalation) = {damageValue}");

      parts.Add($"-> {conversion.fatigue} FAT, {conversion.vitality} VIT");

      if (conversion.wounds > 0)
        parts.Add($", {conversion.wounds} wound{(conversion.wounds > 1 ? "s" : "")}");

      return string.Join(" ", parts);
    }
  }

  /// <summary>
  /// Result of damage conversion lookup.
  /// </summary>
  public class DamageConversionResult
  {
    public int Damage { get; init; }
    public int FatigueDamage { get; init; }
    public int VitalityDamage { get; init; }
    public int Wounds { get; init; }
  }
}
