namespace GameMechanics.Combat
{
  /// <summary>
  /// Request to resolve a melee attack.
  /// </summary>
  public class AttackRequest
  {
    /// <summary>
    /// The attacker's weapon skill Ability Score (Attribute + Skill - 5).
    /// </summary>
    public int AttackerAS { get; init; }

    /// <summary>
    /// The attacker's Physicality Ability Score for damage bonus calculation.
    /// </summary>
    public int AttackerPhysicalityAS { get; init; }

    /// <summary>
    /// The defender's Dodge Ability Score for passive defense calculation.
    /// </summary>
    public int DefenderDodgeAS { get; init; }

    /// <summary>
    /// Number of actions the attacker has already taken this round.
    /// Used to apply multiple action penalty (-1 AS after first action).
    /// </summary>
    public int ActionsThisRound { get; init; }

    /// <summary>
    /// AP boost: additional AP spent to increase AS (+1 per AP).
    /// </summary>
    public int APBoost { get; init; }

    /// <summary>
    /// FAT boost: additional FAT spent to increase AS (+1 per FAT).
    /// </summary>
    public int FATBoost { get; init; }

    /// <summary>
    /// Additional AS modifiers from wounds, effects, equipment, etc.
    /// </summary>
    public int OtherModifiers { get; init; }

    /// <summary>
    /// Creates a new attack request with minimum required parameters.
    /// </summary>
    public static AttackRequest Create(
      int attackerAS,
      int attackerPhysicalityAS,
      int defenderDodgeAS)
    {
      return new AttackRequest
      {
        AttackerAS = attackerAS,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS
      };
    }

    /// <summary>
    /// Calculates the total AS modifier from boosts and penalties.
    /// </summary>
    public int CalculateTotalModifier()
    {
      int modifier = 0;

      // Multiple action penalty: -1 AS for second+ action (not cumulative)
      if (ActionsThisRound > 0)
        modifier -= 1;

      // Boosts: +1 AS per AP or FAT spent
      modifier += APBoost;
      modifier += FATBoost;

      // Other modifiers (wounds, effects, etc.)
      modifier += OtherModifiers;

      return modifier;
    }

    /// <summary>
    /// Gets the effective AS for the attack roll.
    /// </summary>
    public int GetEffectiveAS()
    {
      return AttackerAS + CalculateTotalModifier();
    }
  }
}
