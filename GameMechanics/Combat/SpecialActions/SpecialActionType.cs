namespace GameMechanics.Combat.SpecialActions
{
  /// <summary>
  /// Types of special combat actions.
  /// </summary>
  public enum SpecialActionType
  {
    /// <summary>
    /// Knockback - prevents target from acting for RVE seconds.
    /// No AS penalty, requires knockback-capable weapon.
    /// No damage dealt.
    /// </summary>
    Knockback,

    /// <summary>
    /// Disarm - causes target to drop weapon/item.
    /// -2 AS penalty.
    /// </summary>
    Disarm,

    /// <summary>
    /// Called Shot - targets specific body location.
    /// -2 AS penalty.
    /// </summary>
    CalledShot,

    /// <summary>
    /// Stunning Blow - applies Stunned effect (sets FAT to 0 for SV seconds).
    /// -2 AS penalty.
    /// </summary>
    StunningBlow
  }

  /// <summary>
  /// Constants for special action modifiers.
  /// </summary>
  public static class SpecialActionModifiers
  {
    /// <summary>
    /// AS penalty for Disarm attempts.
    /// </summary>
    public const int DisarmPenalty = -2;

    /// <summary>
    /// AS penalty for Called Shot attempts.
    /// </summary>
    public const int CalledShotPenalty = -2;

    /// <summary>
    /// AS penalty for Stunning Blow attempts.
    /// </summary>
    public const int StunningBlowPenalty = -2;

    /// <summary>
    /// Knockback has no AS penalty.
    /// </summary>
    public const int KnockbackPenalty = 0;

    /// <summary>
    /// Gets the AS penalty for a special action type.
    /// </summary>
    public static int GetPenalty(SpecialActionType actionType)
    {
      return actionType switch
      {
        SpecialActionType.Knockback => KnockbackPenalty,
        SpecialActionType.Disarm => DisarmPenalty,
        SpecialActionType.CalledShot => CalledShotPenalty,
        SpecialActionType.StunningBlow => StunningBlowPenalty,
        _ => 0
      };
    }
  }
}
