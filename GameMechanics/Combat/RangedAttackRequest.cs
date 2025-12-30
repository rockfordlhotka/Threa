namespace GameMechanics.Combat
{
  /// <summary>
  /// Request for a ranged attack resolution.
  /// </summary>
  public class RangedAttackRequest
  {
    /// <summary>
    /// The attacker's ranged weapon Ability Score.
    /// </summary>
    public int AttackerAS { get; init; }

    /// <summary>
    /// The attacker's Physicality AS (for thrown weapons).
    /// </summary>
    public int AttackerPhysicalityAS { get; init; }

    /// <summary>
    /// The weapon's range capabilities.
    /// </summary>
    public WeaponRanges WeaponRanges { get; init; } = WeaponRanges.Shortbow;

    /// <summary>
    /// Distance to target as a range value (distance² = meters).
    /// </summary>
    public int DistanceRangeValue { get; init; }

    /// <summary>
    /// Whether this is a thrown weapon (uses Physicality bonus).
    /// </summary>
    public bool IsThrownWeapon { get; init; }

    /// <summary>
    /// The damage type of the weapon.
    /// </summary>
    public DamageType DamageType { get; init; } = DamageType.Projectile;

    /// <summary>
    /// The damage class of the weapon.
    /// </summary>
    public int DamageClass { get; init; } = 1;

    // === Modifiers ===

    /// <summary>
    /// Whether the target is moving.
    /// </summary>
    public bool TargetMoving { get; init; }

    /// <summary>
    /// Whether the target is prone.
    /// </summary>
    public bool TargetProne { get; init; }

    /// <summary>
    /// Whether the target is crouching.
    /// </summary>
    public bool TargetCrouching { get; init; }

    /// <summary>
    /// The target's size category.
    /// </summary>
    public TargetSize TargetSize { get; init; } = TargetSize.Normal;

    /// <summary>
    /// The target's cover type.
    /// </summary>
    public CoverType Cover { get; init; } = CoverType.None;

    /// <summary>
    /// Whether the attacker is moving.
    /// </summary>
    public bool AttackerMoving { get; init; }

    // === Combat State ===

    /// <summary>
    /// Number of actions already taken this round.
    /// </summary>
    public int ActionsThisRound { get; init; }

    /// <summary>
    /// AP boost applied to this attack.
    /// </summary>
    public int APBoost { get; init; }

    /// <summary>
    /// FAT boost applied to this attack.
    /// </summary>
    public int FATBoost { get; init; }

    /// <summary>
    /// Whether the attacker aimed at this target last round.
    /// </summary>
    public bool HasAimBonus { get; init; }

    /// <summary>
    /// Gets the range category for the current distance.
    /// </summary>
    public RangeCategory GetRangeCategory()
    {
      return WeaponRanges.GetCategory(DistanceRangeValue);
    }

    /// <summary>
    /// Gets the base TV for the current range.
    /// </summary>
    public int GetBaseTV()
    {
      return RangeModifiers.GetBaseTV(GetRangeCategory());
    }

    /// <summary>
    /// Gets the total TV modifier from all conditions.
    /// </summary>
    public int GetTotalModifier()
    {
      return RangeModifiers.CalculateTotalModifier(
        TargetMoving, TargetProne, TargetCrouching,
        TargetSize, Cover, AttackerMoving);
    }

    /// <summary>
    /// Gets the final TV (base + modifiers).
    /// </summary>
    public int GetFinalTV()
    {
      return GetBaseTV() + GetTotalModifier();
    }

    /// <summary>
    /// Gets the effective AS after all modifiers.
    /// </summary>
    public int GetEffectiveAS()
    {
      int @as = AttackerAS;

      // Multiple action penalty (-1 after first action, not cumulative)
      if (ActionsThisRound > 0)
        @as -= 1;

      // Boosts
      @as += APBoost;
      @as += FATBoost;

      // Aim bonus (+2 if aimed last round)
      if (HasAimBonus)
        @as += 2;

      return @as;
    }

    /// <summary>
    /// Whether this attack is out of range.
    /// </summary>
    public bool IsOutOfRange => GetRangeCategory() == RangeCategory.OutOfRange;

    /// <summary>
    /// Creates a simple ranged attack request.
    /// </summary>
    public static RangedAttackRequest Create(
      int attackerAS,
      WeaponRanges weaponRanges,
      int distanceRangeValue,
      bool isThrownWeapon = false,
      int attackerPhysicalityAS = 10)
    {
      return new RangedAttackRequest
      {
        AttackerAS = attackerAS,
        WeaponRanges = weaponRanges,
        DistanceRangeValue = distanceRangeValue,
        IsThrownWeapon = isThrownWeapon,
        AttackerPhysicalityAS = attackerPhysicalityAS
      };
    }
  }
}
