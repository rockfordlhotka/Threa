namespace GameMechanics.Combat.SpecialActions
{
  /// <summary>
  /// Lookup table for Knockback effect duration.
  /// Maps SV to recovery time in seconds (RVE = Result Value Effect).
  /// </summary>
  public static class KnockbackTable
  {
    /// <summary>
    /// Gets the knockback duration in seconds based on SV.
    /// Higher SV = longer knockback.
    /// </summary>
    /// <param name="sv">The Success Value of the knockback attack.</param>
    /// <returns>Duration in seconds that the target cannot act.</returns>
    public static double GetDurationSeconds(int sv)
    {
      if (sv < 0)
        return 0; // Miss - no knockback

      return sv switch
      {
        0 => 0.5,    // Glancing - half second stagger
        1 => 1.0,    // Light knockback
        2 => 1.5,
        3 => 2.0,    // Solid knockback
        4 => 2.5,
        5 => 3.0,    // Strong knockback (1 round)
        6 => 4.0,
        7 => 5.0,
        8 => 6.0,    // Heavy knockback (2 rounds)
        9 => 7.0,
        10 => 8.0,
        _ => 8.0 + (sv - 10) // Scales linearly beyond 10
      };
    }

    /// <summary>
    /// Gets the knockback duration in rounds (3 seconds per round).
    /// </summary>
    public static int GetDurationRounds(int sv)
    {
      double seconds = GetDurationSeconds(sv);
      return (int)System.Math.Ceiling(seconds / 3.0);
    }

    /// <summary>
    /// Gets the critical threshold for knockback.
    /// Critical knockback may knock target prone.
    /// </summary>
    public const int CriticalThreshold = 8;

    /// <summary>
    /// Checks if a knockback result is critical.
    /// </summary>
    public static bool IsCritical(int sv) => sv >= CriticalThreshold;
  }
}
