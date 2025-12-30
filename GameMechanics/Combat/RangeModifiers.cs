namespace GameMechanics.Combat
{
  /// <summary>
  /// Range category for ranged attacks.
  /// Each category has a base TV.
  /// </summary>
  public enum RangeCategory
  {
    /// <summary>
    /// Short range. Base TV 6.
    /// </summary>
    Short,

    /// <summary>
    /// Medium range. Base TV 8.
    /// </summary>
    Medium,

    /// <summary>
    /// Long range. Base TV 10.
    /// </summary>
    Long,

    /// <summary>
    /// Extreme range. Base TV 12.
    /// </summary>
    Extreme,

    /// <summary>
    /// Beyond maximum range - cannot be hit.
    /// </summary>
    OutOfRange
  }

  /// <summary>
  /// Target size modifier for ranged attacks.
  /// </summary>
  public enum TargetSize
  {
    /// <summary>
    /// Normal sized target. No modifier.
    /// </summary>
    Normal,

    /// <summary>
    /// Small target (child, dog). +1 TV.
    /// </summary>
    Small,

    /// <summary>
    /// Tiny target (cat, bird). +2 TV.
    /// </summary>
    Tiny
  }

  /// <summary>
  /// Cover type for ranged attack modifiers.
  /// </summary>
  public enum CoverType
  {
    /// <summary>
    /// No cover. No modifier.
    /// </summary>
    None,

    /// <summary>
    /// Half cover (low wall, table). +1 TV.
    /// </summary>
    Half,

    /// <summary>
    /// Three-quarters cover (doorway, arrow slit). +2 TV.
    /// </summary>
    ThreeQuarters
  }

  /// <summary>
  /// Provides base TV and modifiers for ranged combat.
  /// </summary>
  public static class RangeModifiers
  {
    /// <summary>
    /// Gets the base TV for a range category.
    /// </summary>
    public static int GetBaseTV(RangeCategory category)
    {
      return category switch
      {
        RangeCategory.Short => 6,
        RangeCategory.Medium => 8,
        RangeCategory.Long => 10,
        RangeCategory.Extreme => 12,
        RangeCategory.OutOfRange => int.MaxValue, // Cannot hit
        _ => 8
      };
    }

    /// <summary>
    /// Gets the TV modifier for target movement conditions.
    /// </summary>
    public static int GetTargetMovementModifier(bool isMoving, bool isProne, bool isCrouching)
    {
      int modifier = 0;
      if (isMoving) modifier += 2;
      if (isProne) modifier += 2;
      if (isCrouching) modifier += 2;
      return modifier;
    }

    /// <summary>
    /// Gets the TV modifier for target size.
    /// </summary>
    public static int GetSizeModifier(TargetSize size)
    {
      return size switch
      {
        TargetSize.Normal => 0,
        TargetSize.Small => 1,
        TargetSize.Tiny => 2,
        _ => 0
      };
    }

    /// <summary>
    /// Gets the TV modifier for cover.
    /// </summary>
    public static int GetCoverModifier(CoverType cover)
    {
      return cover switch
      {
        CoverType.None => 0,
        CoverType.Half => 1,
        CoverType.ThreeQuarters => 2,
        _ => 0
      };
    }

    /// <summary>
    /// Gets the TV modifier for attacker conditions.
    /// </summary>
    public static int GetAttackerModifier(bool isMoving)
    {
      return isMoving ? 2 : 0;
    }

    /// <summary>
    /// Calculates total TV modifiers from all conditions.
    /// </summary>
    public static int CalculateTotalModifier(
      bool targetMoving = false,
      bool targetProne = false,
      bool targetCrouching = false,
      TargetSize targetSize = TargetSize.Normal,
      CoverType cover = CoverType.None,
      bool attackerMoving = false)
    {
      int total = 0;
      total += GetTargetMovementModifier(targetMoving, targetProne, targetCrouching);
      total += GetSizeModifier(targetSize);
      total += GetCoverModifier(cover);
      total += GetAttackerModifier(attackerMoving);
      return total;
    }

    /// <summary>
    /// Determines the range category based on distance and weapon ranges.
    /// </summary>
    /// <param name="distanceRangeValue">The distance as a range value (distance² = meters).</param>
    /// <param name="shortRange">Weapon's short range value.</param>
    /// <param name="mediumRange">Weapon's medium range value.</param>
    /// <param name="longRange">Weapon's long range value.</param>
    /// <param name="extremeRange">Weapon's extreme range value.</param>
    /// <returns>The range category.</returns>
    public static RangeCategory DetermineRangeCategory(
      int distanceRangeValue,
      int shortRange,
      int mediumRange,
      int longRange,
      int extremeRange)
    {
      if (distanceRangeValue <= shortRange)
        return RangeCategory.Short;
      if (distanceRangeValue <= mediumRange)
        return RangeCategory.Medium;
      if (distanceRangeValue <= longRange)
        return RangeCategory.Long;
      if (distanceRangeValue <= extremeRange)
        return RangeCategory.Extreme;
      return RangeCategory.OutOfRange;
    }
  }
}
