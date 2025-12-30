namespace GameMechanics.Combat
{
  /// <summary>
  /// Result of a melee attack resolution.
  /// </summary>
  public class AttackResult
  {
    /// <summary>
    /// Whether the attack hit the target.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// The attacker's effective Ability Score (after all modifiers).
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
    /// The Target Value (defender's passive defense: Dodge AS - 1).
    /// </summary>
    public int TV { get; init; }

    /// <summary>
    /// The Success Value (AV - TV). Positive = hit, negative = miss.
    /// </summary>
    public int SV { get; init; }

    /// <summary>
    /// The hit location if the attack succeeded.
    /// </summary>
    public HitLocation? HitLocation { get; init; }

    /// <summary>
    /// The Physicality bonus roll result (only if hit).
    /// </summary>
    public int? PhysicalityRoll { get; init; }

    /// <summary>
    /// The Physicality Result Value (roll - TV 8, only if hit).
    /// </summary>
    public int? PhysicalityRV { get; init; }

    /// <summary>
    /// The Physicality bonus result (SV modifier and any debuffs).
    /// </summary>
    public PhysicalityBonusResult? PhysicalityBonus { get; init; }

    /// <summary>
    /// The final SV after applying Physicality bonus.
    /// </summary>
    public int FinalSV { get; init; }

    /// <summary>
    /// The damage result based on final SV.
    /// </summary>
    public DamageResult? Damage { get; init; }

    /// <summary>
    /// Human-readable summary of the attack result.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Creates a miss result.
    /// </summary>
    public static AttackResult Miss(int effectiveAS, int attackRoll, int av, int tv, int sv)
    {
      return new AttackResult
      {
        IsHit = false,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        FinalSV = sv,
        Summary = $"Miss: AV {av} vs TV {tv} = SV {sv}"
      };
    }

    /// <summary>
    /// Creates a hit result with full damage resolution.
    /// </summary>
    public static AttackResult Hit(
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv,
      HitLocation hitLocation,
      int physicalityRoll,
      int physicalityRV,
      PhysicalityBonusResult physicalityBonus,
      DamageResult damage)
    {
      int finalSV = sv + physicalityBonus.SVModifier;

      return new AttackResult
      {
        IsHit = true,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        HitLocation = hitLocation,
        PhysicalityRoll = physicalityRoll,
        PhysicalityRV = physicalityRV,
        PhysicalityBonus = physicalityBonus,
        FinalSV = finalSV,
        Damage = damage,
        Summary = $"Hit ({hitLocation}): AV {av} vs TV {tv} = SV {sv}, " +
                  $"Physicality {(physicalityBonus.SVModifier >= 0 ? "+" : "")}{physicalityBonus.SVModifier}, " +
                  $"Final SV {finalSV} → {damage.Description}"
      };
    }
  }
}
