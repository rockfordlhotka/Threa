namespace GameMechanics.Combat
{
  /// <summary>
  /// Result of a ranged attack resolution.
  /// </summary>
  public class RangedAttackResult
  {
    /// <summary>
    /// Whether the attack hit the target.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// Whether the target was out of range.
    /// </summary>
    public bool WasOutOfRange { get; init; }

    /// <summary>
    /// The range category of the attack.
    /// </summary>
    public RangeCategory RangeCategory { get; init; }

    /// <summary>
    /// Distance to target as range value.
    /// </summary>
    public int DistanceRangeValue { get; init; }

    /// <summary>
    /// The attacker's effective AS (after all modifiers).
    /// </summary>
    public int EffectiveAS { get; init; }

    /// <summary>
    /// The attack roll result (4dF+).
    /// </summary>
    public int AttackRoll { get; init; }

    /// <summary>
    /// The Attack Value (AS + roll).
    /// </summary>
    public int AV { get; init; }

    /// <summary>
    /// The base TV from range.
    /// </summary>
    public int BaseTV { get; init; }

    /// <summary>
    /// Total TV modifiers applied.
    /// </summary>
    public int TVModifiers { get; init; }

    /// <summary>
    /// The final Target Value (base + modifiers).
    /// </summary>
    public int TV { get; init; }

    /// <summary>
    /// The Success Value (AV - TV).
    /// </summary>
    public int SV { get; init; }

    /// <summary>
    /// The hit location if the attack succeeded.
    /// </summary>
    public HitLocation? HitLocation { get; init; }

    // === Thrown weapon Physicality bonus ===

    /// <summary>
    /// Whether this was a thrown weapon (has Physicality bonus).
    /// </summary>
    public bool IsThrownWeapon { get; init; }

    /// <summary>
    /// The Physicality bonus roll (only for thrown weapons).
    /// </summary>
    public int? PhysicalityRoll { get; init; }

    /// <summary>
    /// The Physicality RV (roll - TV 8).
    /// </summary>
    public int? PhysicalityRV { get; init; }

    /// <summary>
    /// The Physicality bonus result.
    /// </summary>
    public PhysicalityBonusResult? PhysicalityBonus { get; init; }

    /// <summary>
    /// The final SV after Physicality bonus (for thrown weapons).
    /// </summary>
    public int FinalSV { get; init; }

    /// <summary>
    /// The damage result based on final SV.
    /// </summary>
    public DamageResult? Damage { get; init; }

    /// <summary>
    /// Human-readable summary of the attack.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Creates an out-of-range result.
    /// </summary>
    public static RangedAttackResult OutOfRange(int distanceRangeValue)
    {
      return new RangedAttackResult
      {
        IsHit = false,
        WasOutOfRange = true,
        RangeCategory = RangeCategory.OutOfRange,
        DistanceRangeValue = distanceRangeValue,
        Summary = $"Target out of range (distance {distanceRangeValue})"
      };
    }

    /// <summary>
    /// Creates a miss result.
    /// </summary>
    public static RangedAttackResult Miss(
      int effectiveAS,
      int attackRoll,
      int av,
      int baseTV,
      int tvModifiers,
      int tv,
      int sv,
      RangeCategory rangeCategory,
      int distanceRangeValue)
    {
      return new RangedAttackResult
      {
        IsHit = false,
        WasOutOfRange = false,
        RangeCategory = rangeCategory,
        DistanceRangeValue = distanceRangeValue,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        BaseTV = baseTV,
        TVModifiers = tvModifiers,
        TV = tv,
        SV = sv,
        FinalSV = sv,
        Summary = $"Miss at {rangeCategory} range: AV {av} vs TV {tv} (base {baseTV} + mods {tvModifiers}) = SV {sv}"
      };
    }

    /// <summary>
    /// Creates a hit result for a non-thrown weapon.
    /// </summary>
    public static RangedAttackResult Hit(
      int effectiveAS,
      int attackRoll,
      int av,
      int baseTV,
      int tvModifiers,
      int tv,
      int sv,
      RangeCategory rangeCategory,
      int distanceRangeValue,
      HitLocation hitLocation,
      DamageResult damage)
    {
      return new RangedAttackResult
      {
        IsHit = true,
        WasOutOfRange = false,
        RangeCategory = rangeCategory,
        DistanceRangeValue = distanceRangeValue,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        BaseTV = baseTV,
        TVModifiers = tvModifiers,
        TV = tv,
        SV = sv,
        HitLocation = hitLocation,
        IsThrownWeapon = false,
        FinalSV = sv,
        Damage = damage,
        Summary = $"Hit ({hitLocation}) at {rangeCategory} range: AV {av} vs TV {tv} = SV {sv} → {damage.Description}"
      };
    }

    /// <summary>
    /// Creates a hit result for a thrown weapon (includes Physicality bonus).
    /// </summary>
    public static RangedAttackResult ThrownHit(
      int effectiveAS,
      int attackRoll,
      int av,
      int baseTV,
      int tvModifiers,
      int tv,
      int sv,
      RangeCategory rangeCategory,
      int distanceRangeValue,
      HitLocation hitLocation,
      int physicalityRoll,
      int physicalityRV,
      PhysicalityBonusResult physicalityBonus,
      DamageResult damage)
    {
      int finalSV = sv + physicalityBonus.SVModifier;

      return new RangedAttackResult
      {
        IsHit = true,
        WasOutOfRange = false,
        RangeCategory = rangeCategory,
        DistanceRangeValue = distanceRangeValue,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        BaseTV = baseTV,
        TVModifiers = tvModifiers,
        TV = tv,
        SV = sv,
        HitLocation = hitLocation,
        IsThrownWeapon = true,
        PhysicalityRoll = physicalityRoll,
        PhysicalityRV = physicalityRV,
        PhysicalityBonus = physicalityBonus,
        FinalSV = finalSV,
        Damage = damage,
        Summary = $"Thrown hit ({hitLocation}) at {rangeCategory} range: AV {av} vs TV {tv} = SV {sv}, " +
                  $"Physicality {(physicalityBonus.SVModifier >= 0 ? "+" : "")}{physicalityBonus.SVModifier}, " +
                  $"Final SV {finalSV} → {damage.Description}"
      };
    }
  }
}
