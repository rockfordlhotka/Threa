namespace GameMechanics.Combat.SpecialActions
{
  /// <summary>
  /// Result of a special combat action resolution.
  /// </summary>
  public class SpecialActionResult
  {
    /// <summary>
    /// The type of special action attempted.
    /// </summary>
    public SpecialActionType ActionType { get; init; }

    /// <summary>
    /// Whether the action succeeded (SV >= 0).
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The attacker's effective AS (after penalties and modifiers).
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
    /// The Target Value used.
    /// </summary>
    public int TV { get; init; }

    /// <summary>
    /// The Success Value (AV - TV).
    /// </summary>
    public int SV { get; init; }

    /// <summary>
    /// Whether this was a critical success.
    /// Threshold varies by action type.
    /// </summary>
    public bool IsCritical { get; init; }

    // === Knockback Results ===

    /// <summary>
    /// Duration in seconds the target cannot act (Knockback only).
    /// </summary>
    public double? KnockbackDurationSeconds { get; init; }

    /// <summary>
    /// Whether the knockback knocked the target prone (critical).
    /// </summary>
    public bool KnockedProne { get; init; }

    // === Disarm Results ===

    /// <summary>
    /// Whether the target's weapon/item was dropped.
    /// </summary>
    public bool ItemDropped { get; init; }

    /// <summary>
    /// Description of the dropped item.
    /// </summary>
    public string? DroppedItemDescription { get; init; }

    /// <summary>
    /// Whether the item was broken (critical disarm).
    /// </summary>
    public bool ItemBroken { get; init; }

    // === Called Shot Results ===

    /// <summary>
    /// The hit location for Called Shot (or random if missed but still hit).
    /// </summary>
    public HitLocation? HitLocation { get; init; }

    /// <summary>
    /// Whether the called shot hit the intended target.
    /// If false but Success is true, hit a random location.
    /// </summary>
    public bool HitIntendedLocation { get; init; }

    /// <summary>
    /// Damage result for Called Shot (uses Physicality bonus).
    /// </summary>
    public DamageResult? Damage { get; init; }

    /// <summary>
    /// Physicality bonus result for melee special actions.
    /// </summary>
    public PhysicalityBonusResult? PhysicalityBonus { get; init; }

    /// <summary>
    /// Final SV after Physicality bonus (for damage calculation).
    /// </summary>
    public int? FinalSV { get; init; }

    // === Stunning Blow Results ===

    /// <summary>
    /// Duration in seconds the Stunned effect lasts.
    /// </summary>
    public int? StunDurationSeconds { get; init; }

    /// <summary>
    /// Whether the stunning blow caused unconsciousness (critical).
    /// </summary>
    public bool CausedUnconscious { get; init; }

    /// <summary>
    /// Human-readable summary of the result.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    // === Factory Methods ===

    /// <summary>
    /// Creates a miss result for any special action.
    /// </summary>
    public static SpecialActionResult Miss(
      SpecialActionType actionType,
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv)
    {
      return new SpecialActionResult
      {
        ActionType = actionType,
        Success = false,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        Summary = $"{actionType} failed: AV {av} vs TV {tv} = SV {sv}"
      };
    }

    /// <summary>
    /// Creates a Knockback success result.
    /// </summary>
    public static SpecialActionResult KnockbackSuccess(
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv,
      double durationSeconds,
      bool knockedProne)
    {
      return new SpecialActionResult
      {
        ActionType = SpecialActionType.Knockback,
        Success = true,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        IsCritical = knockedProne,
        KnockbackDurationSeconds = durationSeconds,
        KnockedProne = knockedProne,
        Summary = $"Knockback success: Target cannot act for {durationSeconds:F1}s" +
                  (knockedProne ? " and is knocked prone" : "")
      };
    }

    /// <summary>
    /// Creates a Disarm success result.
    /// </summary>
    public static SpecialActionResult DisarmSuccess(
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv,
      string itemDescription,
      bool itemBroken)
    {
      return new SpecialActionResult
      {
        ActionType = SpecialActionType.Disarm,
        Success = true,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        IsCritical = itemBroken,
        ItemDropped = true,
        DroppedItemDescription = itemDescription,
        ItemBroken = itemBroken,
        Summary = $"Disarm success: {itemDescription} dropped" +
                  (itemBroken ? " and broken!" : "")
      };
    }

    /// <summary>
    /// Creates a Called Shot success result.
    /// </summary>
    public static SpecialActionResult CalledShotSuccess(
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv,
      HitLocation hitLocation,
      bool hitIntendedLocation,
      PhysicalityBonusResult physBonus,
      int finalSV,
      DamageResult damage)
    {
      string locationInfo = hitIntendedLocation
        ? $"Hit intended location: {hitLocation}"
        : $"Missed intended location, hit {hitLocation}";

      return new SpecialActionResult
      {
        ActionType = SpecialActionType.CalledShot,
        Success = true,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        IsCritical = finalSV >= 8,
        HitLocation = hitLocation,
        HitIntendedLocation = hitIntendedLocation,
        PhysicalityBonus = physBonus,
        FinalSV = finalSV,
        Damage = damage,
        Summary = $"Called Shot: {locationInfo}, SV {sv} + Phys {physBonus.SVModifier} = {finalSV} → {damage.Description}"
      };
    }

    /// <summary>
    /// Creates a Stunning Blow success result.
    /// </summary>
    public static SpecialActionResult StunningBlowSuccess(
      int effectiveAS,
      int attackRoll,
      int av,
      int tv,
      int sv,
      int stunDurationSeconds,
      bool causedUnconscious)
    {
      return new SpecialActionResult
      {
        ActionType = SpecialActionType.StunningBlow,
        Success = true,
        EffectiveAS = effectiveAS,
        AttackRoll = attackRoll,
        AV = av,
        TV = tv,
        SV = sv,
        IsCritical = causedUnconscious,
        StunDurationSeconds = stunDurationSeconds,
        CausedUnconscious = causedUnconscious,
        Summary = causedUnconscious
          ? $"Stunning Blow: Target knocked unconscious!"
          : $"Stunning Blow: Target stunned for {stunDurationSeconds}s (FAT = 0)"
      };
    }
  }
}
