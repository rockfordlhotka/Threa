namespace GameMechanics.Combat
{
  /// <summary>
  /// Resolves ranged attacks including bows, crossbows, and thrown weapons.
  /// </summary>
  public class RangedAttackResolver
  {
    private const int PhysicalityTV = 8;

    private readonly IDiceRoller _diceRoller;

    public RangedAttackResolver(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
    }

    /// <summary>
    /// Resolves a ranged attack using calculated TV.
    /// </summary>
    public RangedAttackResult Resolve(RangedAttackRequest request)
    {
      // Check if out of range
      if (request.IsOutOfRange)
      {
        return RangedAttackResult.OutOfRange(request.DistanceRangeValue);
      }

      var rangeCategory = request.GetRangeCategory();
      int baseTV = request.GetBaseTV();
      int tvModifiers = request.GetTotalModifier();
      int tv = baseTV + tvModifiers;

      int effectiveAS = request.GetEffectiveAS();
      int attackRoll = _diceRoller.Roll4dFPlus();
      int av = effectiveAS + attackRoll;
      int sv = av - tv;

      // Miss
      if (sv < 0)
      {
        return RangedAttackResult.Miss(
          effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
          rangeCategory, request.DistanceRangeValue);
      }

      // Hit - determine location
      int locationRoll = _diceRoller.Roll(1, 24);
      var hitLocation = HitLocationCalculator.MapRollToLocation(locationRoll);

      // Thrown weapons get Physicality bonus
      if (request.IsThrownWeapon)
      {
        int physRoll = _diceRoller.Roll4dFPlus();
        int physTotal = request.AttackerPhysicalityAS + physRoll;
        int physRV = physTotal - PhysicalityTV;
        var physBonus = CombatResultTables.GetPhysicalityBonus(physRV);

        int finalSV = sv + physBonus.SVModifier;
        var damage = CombatResultTables.GetDamage(finalSV);

        return RangedAttackResult.ThrownHit(
          effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
          rangeCategory, request.DistanceRangeValue, hitLocation,
          physRoll, physRV, physBonus, damage);
      }

      // Non-thrown ranged - no Physicality bonus
      var baseDamage = CombatResultTables.GetDamage(sv);
      return RangedAttackResult.Hit(
        effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
        rangeCategory, request.DistanceRangeValue, hitLocation, baseDamage);
    }

    /// <summary>
    /// Resolves a ranged attack with an explicit TV (e.g., from active defense).
    /// </summary>
    public RangedAttackResult ResolveWithTV(RangedAttackRequest request, int tv)
    {
      // Check if out of range
      if (request.IsOutOfRange)
      {
        return RangedAttackResult.OutOfRange(request.DistanceRangeValue);
      }

      var rangeCategory = request.GetRangeCategory();
      int baseTV = request.GetBaseTV();
      int tvModifiers = tv - baseTV; // Back-calculate modifiers for reporting

      int effectiveAS = request.GetEffectiveAS();
      int attackRoll = _diceRoller.Roll4dFPlus();
      int av = effectiveAS + attackRoll;
      int sv = av - tv;

      // Miss
      if (sv < 0)
      {
        return RangedAttackResult.Miss(
          effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
          rangeCategory, request.DistanceRangeValue);
      }

      // Hit - determine location
      int locationRoll = _diceRoller.Roll(1, 24);
      var hitLocation = HitLocationCalculator.MapRollToLocation(locationRoll);

      // Thrown weapons get Physicality bonus
      if (request.IsThrownWeapon)
      {
        int physRoll = _diceRoller.Roll4dFPlus();
        int physTotal = request.AttackerPhysicalityAS + physRoll;
        int physRV = physTotal - PhysicalityTV;
        var physBonus = CombatResultTables.GetPhysicalityBonus(physRV);

        int finalSV = sv + physBonus.SVModifier;
        var damage = CombatResultTables.GetDamage(finalSV);

        return RangedAttackResult.ThrownHit(
          effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
          rangeCategory, request.DistanceRangeValue, hitLocation,
          physRoll, physRV, physBonus, damage);
      }

      // Non-thrown ranged - no Physicality bonus
      var baseDamage = CombatResultTables.GetDamage(sv);
      return RangedAttackResult.Hit(
        effectiveAS, attackRoll, av, baseTV, tvModifiers, tv, sv,
        rangeCategory, request.DistanceRangeValue, hitLocation, baseDamage);
    }
  }
}
