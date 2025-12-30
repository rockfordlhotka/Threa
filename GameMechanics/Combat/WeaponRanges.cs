namespace GameMechanics.Combat
{
  /// <summary>
  /// Represents a ranged weapon's range capabilities.
  /// Range values are power-of-2: actual distance (meters) = RangeValue²
  /// </summary>
  public class WeaponRanges
  {
    /// <summary>
    /// Short range value. Base TV 6.
    /// </summary>
    public int Short { get; init; }

    /// <summary>
    /// Medium range value. Base TV 8.
    /// </summary>
    public int Medium { get; init; }

    /// <summary>
    /// Long range value. Base TV 10.
    /// </summary>
    public int Long { get; init; }

    /// <summary>
    /// Extreme range value. Base TV 12.
    /// </summary>
    public int Extreme { get; init; }

    /// <summary>
    /// Gets the range category for a given distance.
    /// </summary>
    public RangeCategory GetCategory(int distanceRangeValue)
    {
      return RangeModifiers.DetermineRangeCategory(
        distanceRangeValue, Short, Medium, Long, Extreme);
    }

    /// <summary>
    /// Creates standard shortbow ranges (3/5/7/9).
    /// </summary>
    public static WeaponRanges Shortbow => new()
    {
      Short = 3,   // 9m
      Medium = 5,  // 25m
      Long = 7,    // 49m
      Extreme = 9  // 81m
    };

    /// <summary>
    /// Creates standard longbow ranges (4/6/8/10).
    /// </summary>
    public static WeaponRanges Longbow => new()
    {
      Short = 4,   // 16m
      Medium = 6,  // 36m
      Long = 8,    // 64m
      Extreme = 10 // 100m
    };

    /// <summary>
    /// Creates standard crossbow ranges (5/7/9/11).
    /// </summary>
    public static WeaponRanges Crossbow => new()
    {
      Short = 5,   // 25m
      Medium = 7,  // 49m
      Long = 9,    // 81m
      Extreme = 11 // 121m
    };

    /// <summary>
    /// Creates thrown dagger ranges (1/2/3/4).
    /// </summary>
    public static WeaponRanges ThrownDagger => new()
    {
      Short = 1,  // 1m
      Medium = 2, // 4m
      Long = 3,   // 9m
      Extreme = 4 // 16m
    };

    /// <summary>
    /// Creates thrown javelin ranges (2/4/5/6).
    /// </summary>
    public static WeaponRanges ThrownJavelin => new()
    {
      Short = 2,  // 4m
      Medium = 4, // 16m
      Long = 5,   // 25m
      Extreme = 6 // 36m
    };
  }
}
