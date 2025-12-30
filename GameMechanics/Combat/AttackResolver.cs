namespace GameMechanics.Combat
{
  /// <summary>
  /// Resolves melee attack actions.
  /// </summary>
  public class AttackResolver
  {
    private const int PhysicalityTV = 8;

    private readonly IDiceRoller _diceRoller;
    private readonly HitLocationCalculator _hitLocationCalculator;

    public AttackResolver(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
      _hitLocationCalculator = new HitLocationCalculator(diceRoller);
    }

    /// <summary>
    /// Resolves a melee attack against passive defense.
    /// </summary>
    /// <param name="request">The attack request containing all relevant values.</param>
    /// <returns>The attack result with full resolution details.</returns>
    public AttackResult Resolve(AttackRequest request)
    {
      // Step 1: Calculate effective AS with all modifiers
      int effectiveAS = request.GetEffectiveAS();

      // Step 2: Roll attack (4dF+)
      int attackRoll = _diceRoller.Roll4dFPlus();
      int av = effectiveAS + attackRoll;

      // Step 3: Calculate passive defense TV (Dodge AS - 1)
      int tv = request.DefenderDodgeAS - 1;

      // Step 4: Calculate SV
      int sv = av - tv;

      // Step 5: Determine hit or miss
      if (sv < 0)
      {
        return AttackResult.Miss(effectiveAS, attackRoll, av, tv, sv);
      }

      // Step 6: Determine hit location
      HitLocation hitLocation = _hitLocationCalculator.DetermineHitLocation();

      // Step 7: Roll Physicality bonus (automatic, free action)
      int physicalityRoll = request.AttackerPhysicalityAS + _diceRoller.Roll4dFPlus();
      int physicalityRV = physicalityRoll - PhysicalityTV;
      PhysicalityBonusResult physicalityBonus = CombatResultTables.GetPhysicalityBonus(physicalityRV);

      // Step 8: Calculate final SV with Physicality bonus
      int finalSV = sv + physicalityBonus.SVModifier;

      // Step 9: Look up damage
      DamageResult damage = CombatResultTables.GetDamage(finalSV);

      return AttackResult.Hit(
        effectiveAS,
        attackRoll,
        av,
        tv,
        sv,
        hitLocation,
        physicalityRoll,
        physicalityRV,
        physicalityBonus,
        damage);
    }

    /// <summary>
    /// Resolves a melee attack with explicit TV (for active defense or other scenarios).
    /// </summary>
    /// <param name="request">The attack request.</param>
    /// <param name="tv">The target value to use instead of passive defense.</param>
    /// <returns>The attack result.</returns>
    public AttackResult ResolveWithTV(AttackRequest request, int tv)
    {
      // Step 1: Calculate effective AS with all modifiers
      int effectiveAS = request.GetEffectiveAS();

      // Step 2: Roll attack (4dF+)
      int attackRoll = _diceRoller.Roll4dFPlus();
      int av = effectiveAS + attackRoll;

      // Step 3: Calculate SV
      int sv = av - tv;

      // Step 4: Determine hit or miss
      if (sv < 0)
      {
        return AttackResult.Miss(effectiveAS, attackRoll, av, tv, sv);
      }

      // Step 5: Determine hit location
      HitLocation hitLocation = _hitLocationCalculator.DetermineHitLocation();

      // Step 6: Roll Physicality bonus (automatic, free action)
      int physicalityRoll = request.AttackerPhysicalityAS + _diceRoller.Roll4dFPlus();
      int physicalityRV = physicalityRoll - PhysicalityTV;
      PhysicalityBonusResult physicalityBonus = CombatResultTables.GetPhysicalityBonus(physicalityRV);

      // Step 7: Calculate final SV with Physicality bonus
      int finalSV = sv + physicalityBonus.SVModifier;

      // Step 8: Look up damage
      DamageResult damage = CombatResultTables.GetDamage(finalSV);

      return AttackResult.Hit(
        effectiveAS,
        attackRoll,
        av,
        tv,
        sv,
        hitLocation,
        physicalityRoll,
        physicalityRV,
        physicalityBonus,
        damage);
    }
  }
}
