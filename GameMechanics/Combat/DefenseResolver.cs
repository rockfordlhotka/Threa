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
    /// </summary>
    private DefenseResult ResolvePassive(DefenseRequest request)
    {
      return DefenseResult.Passive(request.DodgeAS);
    }

    /// <summary>
    /// Resolves active dodge defense.
    /// </summary>
    private DefenseResult ResolveDodge(DefenseRequest request)
    {
      int roll = _diceRoller.Roll4dFPlus();
      return DefenseResult.ActiveDodge(request.DodgeAS, roll);
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

      int roll = _diceRoller.Roll4dFPlus();
      return DefenseResult.ActiveParry(request.ParryAS, roll, request.IsInParryMode);
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
