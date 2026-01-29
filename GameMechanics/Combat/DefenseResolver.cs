using GameMechanics.Effects.Behaviors;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Resolves defense actions and calculates TV.
  /// </summary>
  public class DefenseResolver
  {
    private const int ShieldBlockTV = 8;

    private readonly IDiceRoller _diceRoller;

    public DefenseResolver(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
    }

    /// <summary>
    /// Calculates the defense result based on the defense request.
    /// </summary>
    /// <param name="request">The defense request.</param>
    /// <returns>The defense result with calculated TV.</returns>
    public DefenseResult Resolve(DefenseRequest request)
    {
      return request.DefenseType switch
      {
        DefenseType.Passive => ResolvePassive(request),
        DefenseType.Dodge => ResolveDodge(request),
        DefenseType.Parry => ResolveParry(request),
        DefenseType.ShieldBlock => ResolveShieldBlock(request),
        _ => DefenseResult.Invalid(request.DefenseType, "Unknown defense type")
      };
    }

    /// <summary>
    /// Calculates passive defense TV.
    /// For concentrating defenders, a concentration check is performed after damage calculation.
    /// </summary>
    private DefenseResult ResolvePassive(DefenseRequest request)
    {
      var result = DefenseResult.Passive(request.DodgeAS);

      // Passive defense triggers concentration check AFTER damage is determined
      // The caller sets DamageDealt and AttackerAV when calling this
      if (request.Defender != null && ConcentrationBehavior.IsConcentrating(request.Defender))
      {
        // Perform concentration check: Focus AS + 4dF+ vs attacker AV
        // Damage penalty: -1 per 2 damage dealt
        var checkResult = request.Defender.CheckConcentration(
          request.AttackerAV,
          request.DamageDealt,
          _diceRoller);

        result.ConcentrationCheck = checkResult;
        result.ConcentrationBroken = !checkResult.Success;

        // Auto-break on incapacitation: if health pools would be depleted, break concentration
        // This happens even if the concentration check succeeded
        if (!result.ConcentrationBroken && ConcentrationBehavior.IsConcentrating(request.Defender))
        {
          CheckHealthDepletionBreak(request.Defender, result);
        }
      }

      return result;
    }

    /// <summary>
    /// Checks if the defender's health pools are depleted and breaks concentration if so.
    /// Called after concentration check for passive defense.
    /// </summary>
    private static void CheckHealthDepletionBreak(CharacterEdit defender, DefenseResult result)
    {
      // Check current health pools
      // Using current damage values - if FAT or VIT is already at/below 0 effective value
      bool fatigueExhausted = defender.Fatigue.Value <= defender.Fatigue.PendingDamage;
      bool vitalityExhausted = defender.Vitality.Value <= defender.Vitality.PendingDamage;

      if (fatigueExhausted || vitalityExhausted)
      {
        ConcentrationBehavior.BreakConcentration(
          defender,
          "Incapacitated - concentration broken");
        result.ConcentrationBroken = true;
      }
    }

    /// <summary>
    /// Resolves active dodge defense.
    /// </summary>
    private DefenseResult ResolveDodge(DefenseRequest request)
    {
      // Active defense breaks concentration before the roll
      bool concentrationBroken = false;
      if (request.Defender != null && ConcentrationBehavior.IsConcentrating(request.Defender))
      {
        ConcentrationBehavior.BreakConcentration(
          request.Defender,
          "Chose active defense - concentration broken");
        concentrationBroken = true;
      }

      int roll = _diceRoller.Roll4dFPlus();
      var result = DefenseResult.ActiveDodge(request.DodgeAS, roll);
      result.ConcentrationBroken = concentrationBroken;
      return result;
    }

    /// <summary>
    /// Resolves parry defense.
    /// </summary>
    private DefenseResult ResolveParry(DefenseRequest request)
    {
      // Cannot parry ranged attacks
      if (request.IsRangedAttack)
      {
        return DefenseResult.Invalid(
          DefenseType.Parry,
          "Cannot parry ranged attacks");
      }

      // Active defense breaks concentration before the roll
      bool concentrationBroken = false;
      if (request.Defender != null && ConcentrationBehavior.IsConcentrating(request.Defender))
      {
        ConcentrationBehavior.BreakConcentration(
          request.Defender,
          "Chose active defense - concentration broken");
        concentrationBroken = true;
      }

      int roll = _diceRoller.Roll4dFPlus();
      var result = DefenseResult.ActiveParry(request.ParryAS, roll, request.IsInParryMode);
      result.ConcentrationBroken = concentrationBroken;
      return result;
    }

    /// <summary>
    /// Resolves shield block.
    /// Shield block is a "free action" - it doesn't replace other defense,
    /// but adds damage absorption if successful.
    /// </summary>
    /// <param name="request">The defense request.</param>
    /// <param name="baseTV">The TV from the primary defense (passive, dodge, or parry).</param>
    public DefenseResult ResolveShieldBlock(DefenseRequest request, int baseTV)
    {
      int roll = _diceRoller.Roll4dFPlus();
      return DefenseResult.ShieldBlockResult(request.ShieldAS, roll, baseTV);
    }

    /// <summary>
    /// Resolves shield block when called directly (uses TV 0 as placeholder).
    /// </summary>
    private DefenseResult ResolveShieldBlock(DefenseRequest request)
    {
      return ResolveShieldBlock(request, 0);
    }

    /// <summary>
    /// Resolves a complete defense with optional shield block.
    /// </summary>
    /// <param name="primaryDefense">The primary defense request (passive, dodge, or parry).</param>
    /// <param name="hasShield">Whether the defender has a shield equipped.</param>
    /// <param name="shieldAS">The shield skill AS if a shield is equipped.</param>
    /// <returns>A tuple of (primary defense result, shield block result or null).</returns>
    public (DefenseResult Primary, DefenseResult? ShieldBlock) ResolveWithShield(
      DefenseRequest primaryDefense,
      bool hasShield,
      int shieldAS = 0)
    {
      var primary = Resolve(primaryDefense);

      if (!hasShield || !primary.IsValid)
      {
        return (primary, null);
      }

      var shieldRequest = DefenseRequest.ShieldBlock(shieldAS);
      var shield = ResolveShieldBlock(shieldRequest, primary.TV);

      return (primary, shield);
    }
  }
}
