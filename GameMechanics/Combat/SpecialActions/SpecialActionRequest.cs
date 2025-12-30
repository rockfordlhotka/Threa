namespace GameMechanics.Combat.SpecialActions
{
  /// <summary>
  /// Request for a special combat action resolution.
  /// </summary>
  public class SpecialActionRequest
  {
    /// <summary>
    /// The type of special action being attempted.
    /// </summary>
    public SpecialActionType ActionType { get; init; }

    /// <summary>
    /// The attacker's weapon/unarmed Ability Score.
    /// </summary>
    public int AttackerAS { get; init; }

    /// <summary>
    /// The attacker's Physicality AS (for melee Physicality bonus).
    /// </summary>
    public int AttackerPhysicalityAS { get; init; }

    /// <summary>
    /// The defender's Dodge AS (for passive defense TV calculation).
    /// </summary>
    public int DefenderDodgeAS { get; init; }

    /// <summary>
    /// Number of actions already taken this round.
    /// </summary>
    public int ActionsThisRound { get; init; }

    /// <summary>
    /// AP boost applied to this action.
    /// </summary>
    public int APBoost { get; init; }

    /// <summary>
    /// FAT boost applied to this action.
    /// </summary>
    public int FATBoost { get; init; }

    // === Called Shot Specific ===

    /// <summary>
    /// The target location for a Called Shot.
    /// Required when ActionType is CalledShot.
    /// </summary>
    public HitLocation? TargetLocation { get; init; }

    // === Disarm Specific ===

    /// <summary>
    /// Description of the item being targeted for disarm.
    /// Required when ActionType is Disarm.
    /// </summary>
    public string? TargetItemDescription { get; init; }

    // === Knockback Specific ===

    /// <summary>
    /// Whether the weapon has knockback capability.
    /// Required for Knockback action type.
    /// </summary>
    public bool WeaponHasKnockback { get; init; }

    /// <summary>
    /// Gets the effective AS after all modifiers including special action penalty.
    /// </summary>
    public int GetEffectiveAS()
    {
      int @as = AttackerAS;

      // Special action penalty
      @as += SpecialActionModifiers.GetPenalty(ActionType);

      // Multiple action penalty (-1 after first action, not cumulative)
      if (ActionsThisRound > 0)
        @as -= 1;

      // Boosts
      @as += APBoost;
      @as += FATBoost;

      return @as;
    }

    /// <summary>
    /// Gets the passive defense TV.
    /// </summary>
    public int GetPassiveDefenseTV()
    {
      return DefenderDodgeAS - 1;
    }

    /// <summary>
    /// Creates a Knockback action request.
    /// </summary>
    public static SpecialActionRequest CreateKnockback(
      int attackerAS,
      int attackerPhysicalityAS,
      int defenderDodgeAS,
      bool weaponHasKnockback = true)
    {
      return new SpecialActionRequest
      {
        ActionType = SpecialActionType.Knockback,
        AttackerAS = attackerAS,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS,
        WeaponHasKnockback = weaponHasKnockback
      };
    }

    /// <summary>
    /// Creates a Disarm action request.
    /// </summary>
    public static SpecialActionRequest CreateDisarm(
      int attackerAS,
      int attackerPhysicalityAS,
      int defenderDodgeAS,
      string targetItemDescription)
    {
      return new SpecialActionRequest
      {
        ActionType = SpecialActionType.Disarm,
        AttackerAS = attackerAS,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS,
        TargetItemDescription = targetItemDescription
      };
    }

    /// <summary>
    /// Creates a Called Shot action request.
    /// </summary>
    public static SpecialActionRequest CreateCalledShot(
      int attackerAS,
      int attackerPhysicalityAS,
      int defenderDodgeAS,
      HitLocation targetLocation)
    {
      return new SpecialActionRequest
      {
        ActionType = SpecialActionType.CalledShot,
        AttackerAS = attackerAS,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS,
        TargetLocation = targetLocation
      };
    }

    /// <summary>
    /// Creates a Stunning Blow action request.
    /// </summary>
    public static SpecialActionRequest CreateStunningBlow(
      int attackerAS,
      int attackerPhysicalityAS,
      int defenderDodgeAS)
    {
      return new SpecialActionRequest
      {
        ActionType = SpecialActionType.StunningBlow,
        AttackerAS = attackerAS,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS
      };
    }
  }
}
