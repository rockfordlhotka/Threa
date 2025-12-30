namespace GameMechanics.Combat
{
  /// <summary>
  /// Request to calculate a defense TV.
  /// </summary>
  public class DefenseRequest
  {
    /// <summary>
    /// The type of defense being used.
    /// </summary>
    public DefenseType DefenseType { get; init; }

    /// <summary>
    /// The defender's Dodge Ability Score (Attribute + Skill - 5).
    /// Used for Passive and Dodge defenses.
    /// </summary>
    public int DodgeAS { get; init; }

    /// <summary>
    /// The defender's Weapon or Shield Ability Score for parrying.
    /// Used for Parry defense.
    /// </summary>
    public int ParryAS { get; init; }

    /// <summary>
    /// The defender's Shield Ability Score.
    /// Used for Shield Block.
    /// </summary>
    public int ShieldAS { get; init; }

    /// <summary>
    /// Whether the defender is currently in parry mode.
    /// If true, parry defenses are free (no AP/FAT cost).
    /// </summary>
    public bool IsInParryMode { get; init; }

    /// <summary>
    /// Whether this is a ranged attack.
    /// Parry cannot be used against ranged attacks.
    /// </summary>
    public bool IsRangedAttack { get; init; }

    /// <summary>
    /// Creates a passive defense request.
    /// </summary>
    public static DefenseRequest Passive(int dodgeAS)
    {
      return new DefenseRequest
      {
        DefenseType = DefenseType.Passive,
        DodgeAS = dodgeAS
      };
    }

    /// <summary>
    /// Creates an active dodge defense request.
    /// </summary>
    public static DefenseRequest Dodge(int dodgeAS)
    {
      return new DefenseRequest
      {
        DefenseType = DefenseType.Dodge,
        DodgeAS = dodgeAS
      };
    }

    /// <summary>
    /// Creates a parry defense request.
    /// </summary>
    public static DefenseRequest Parry(int parryAS, bool isInParryMode)
    {
      return new DefenseRequest
      {
        DefenseType = DefenseType.Parry,
        ParryAS = parryAS,
        IsInParryMode = isInParryMode
      };
    }

    /// <summary>
    /// Creates a shield block request.
    /// </summary>
    public static DefenseRequest ShieldBlock(int shieldAS)
    {
      return new DefenseRequest
      {
        DefenseType = DefenseType.ShieldBlock,
        ShieldAS = shieldAS
      };
    }
  }
}
