namespace GameMechanics.Combat
{
  /// <summary>
  /// Body locations that can be hit in combat.
  /// </summary>
  public enum HitLocation
  {
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
  }

  /// <summary>
  /// Calculates hit locations with proper probability distribution.
  /// Uses a single d24-equivalent roll optimized from the two-step d12 system.
  /// </summary>
  /// <remarks>
  /// Original probabilities (d12 with reroll on 1):
  /// - Head: 1/24 (4.17%)
  /// - Torso: 11/24 (45.83%)
  /// - Left Arm: 2/24 (8.33%)
  /// - Right Arm: 2/24 (8.33%)
  /// - Left Leg: 4/24 (16.67%)
  /// - Right Leg: 4/24 (16.67%)
  /// </remarks>
  public class HitLocationCalculator
  {
    private readonly IDiceRoller _diceRoller;

    public HitLocationCalculator(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
    }

    /// <summary>
    /// Determines hit location using optimized single-roll method.
    /// </summary>
    /// <returns>The hit location.</returns>
    public HitLocation DetermineHitLocation()
    {
      // Roll 1-24 (equivalent to d24)
      int roll = _diceRoller.Roll(1, 24);
      return MapRollToLocation(roll);
    }

    /// <summary>
    /// Maps a 1-24 roll to a hit location with correct probability distribution.
    /// </summary>
    public static HitLocation MapRollToLocation(int roll)
    {
      return roll switch
      {
        1 => HitLocation.Head,           // 1/24 = 4.17%
        >= 2 and <= 12 => HitLocation.Torso,    // 11/24 = 45.83%
        >= 13 and <= 14 => HitLocation.LeftArm,  // 2/24 = 8.33%
        >= 15 and <= 16 => HitLocation.RightArm, // 2/24 = 8.33%
        >= 17 and <= 20 => HitLocation.LeftLeg,  // 4/24 = 16.67%
        >= 21 and <= 24 => HitLocation.RightLeg, // 4/24 = 16.67%
        _ => HitLocation.Torso // Fallback for invalid rolls
      };
    }

    /// <summary>
    /// Gets the probability of hitting a specific location.
    /// </summary>
    public static double GetLocationProbability(HitLocation location)
    {
      return location switch
      {
        HitLocation.Head => 1.0 / 24.0,
        HitLocation.Torso => 11.0 / 24.0,
        HitLocation.LeftArm => 2.0 / 24.0,
        HitLocation.RightArm => 2.0 / 24.0,
        HitLocation.LeftLeg => 4.0 / 24.0,
        HitLocation.RightLeg => 4.0 / 24.0,
        _ => 0.0
      };
    }
  }
}
