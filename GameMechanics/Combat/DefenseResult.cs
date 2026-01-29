using GameMechanics.Effects.Behaviors;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Result of a defense calculation.
  /// </summary>
  public class DefenseResult
  {
    /// <summary>
    /// Result of concentration check if defender was concentrating and used passive defense.
    /// Null if not concentrating or used active defense.
    /// </summary>
    public ConcentrationCheckResult? ConcentrationCheck { get; set; }

    /// <summary>
    /// True if concentration was broken during this defense (either by active defense or failed check).
    /// </summary>
    public bool ConcentrationBroken { get; set; }

    /// <summary>
    /// The type of defense used.
    /// </summary>
    public DefenseType DefenseType { get; init; }

    /// <summary>
    /// The calculated Target Value for the attack to beat.
    /// </summary>
    public int TV { get; init; }

    /// <summary>
    /// The defense roll result (4dF+). Null for passive defense.
    /// </summary>
    public int? DefenseRoll { get; init; }

    /// <summary>
    /// The Ability Score used for the defense.
    /// </summary>
    public int AS { get; init; }

    /// <summary>
    /// Whether this defense costs AP/FAT.
    /// False for passive defense or parry while in parry mode.
    /// </summary>
    public bool CostsAction { get; init; }

    /// <summary>
    /// Whether the defense is valid.
    /// False if trying to parry a ranged attack, for example.
    /// </summary>
    public bool IsValid { get; init; } = true;

    /// <summary>
    /// Error message if the defense is invalid.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Human-readable summary of the defense.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    // === Shield Block specific properties ===

    /// <summary>
    /// For shield block: the roll result vs TV 8.
    /// </summary>
    public int? ShieldBlockRoll { get; init; }

    /// <summary>
    /// For shield block: whether the block succeeded.
    /// </summary>
    public bool? ShieldBlockSuccess { get; init; }

    /// <summary>
    /// For shield block: the Result Value (roll - 8) for absorption calculation.
    /// </summary>
    public int? ShieldBlockRV { get; init; }

    /// <summary>
    /// Creates a passive defense result.
    /// </summary>
    public static DefenseResult Passive(int dodgeAS)
    {
      int tv = dodgeAS - 1;
      return new DefenseResult
      {
        DefenseType = DefenseType.Passive,
        AS = dodgeAS,
        TV = tv,
        DefenseRoll = null,
        CostsAction = false,
        Summary = $"Passive defense: TV {tv} (Dodge AS {dodgeAS} - 1)"
      };
    }

    /// <summary>
    /// Creates an active dodge result.
    /// </summary>
    public static DefenseResult ActiveDodge(int dodgeAS, int roll)
    {
      int tv = dodgeAS + roll;
      return new DefenseResult
      {
        DefenseType = DefenseType.Dodge,
        AS = dodgeAS,
        DefenseRoll = roll,
        TV = tv,
        CostsAction = true,
        Summary = $"Active dodge: TV {tv} (Dodge AS {dodgeAS} + roll {roll})"
      };
    }

    /// <summary>
    /// Creates a parry result.
    /// </summary>
    public static DefenseResult ActiveParry(int parryAS, int roll, bool isInParryMode)
    {
      int tv = parryAS + roll;
      return new DefenseResult
      {
        DefenseType = DefenseType.Parry,
        AS = parryAS,
        DefenseRoll = roll,
        TV = tv,
        CostsAction = !isInParryMode, // Free if already in parry mode
        Summary = $"Parry: TV {tv} (Parry AS {parryAS} + roll {roll})" +
                  (isInParryMode ? " [free - parry mode]" : "")
      };
    }

    /// <summary>
    /// Creates a shield block result.
    /// Shield block doesn't set TV - it's used for damage absorption.
    /// </summary>
    public static DefenseResult ShieldBlockResult(int shieldAS, int roll, int baseTV)
    {
      int totalRoll = shieldAS + roll;
      bool success = totalRoll >= 8;
      int rv = totalRoll - 8;

      return new DefenseResult
      {
        DefenseType = DefenseType.ShieldBlock,
        AS = shieldAS,
        TV = baseTV, // Shield block doesn't change TV
        DefenseRoll = roll,
        CostsAction = false, // Shield block is free like Physicality bonus
        ShieldBlockRoll = totalRoll,
        ShieldBlockSuccess = success,
        ShieldBlockRV = success ? rv : null,
        Summary = success
          ? $"Shield block succeeded: RV {rv} (Shield AS {shieldAS} + roll {roll} = {totalRoll} vs TV 8)"
          : $"Shield block failed: (Shield AS {shieldAS} + roll {roll} = {totalRoll} vs TV 8)"
      };
    }

    /// <summary>
    /// Creates an invalid defense result.
    /// </summary>
    public static DefenseResult Invalid(DefenseType type, string errorMessage)
    {
      return new DefenseResult
      {
        DefenseType = type,
        IsValid = false,
        ErrorMessage = errorMessage,
        Summary = $"Invalid defense: {errorMessage}"
      };
    }
  }
}
