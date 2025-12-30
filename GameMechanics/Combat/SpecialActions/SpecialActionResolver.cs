using System;
using System.Threading.Tasks;
using GameMechanics.Effects;

namespace GameMechanics.Combat.SpecialActions
{
  /// <summary>
  /// Resolves special combat actions and applies effects.
  /// </summary>
  public class SpecialActionResolver
  {
    private const int PhysicalityTV = 8;
    private const int DisarmCriticalThreshold = 8;
    private const int StunCriticalThreshold = 10;

    private readonly IDiceRoller _diceRoller;
    private readonly EffectManager? _effectManager;

    /// <summary>
    /// Creates a resolver without effect management (for testing).
    /// </summary>
    public SpecialActionResolver(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
    }

    /// <summary>
    /// Creates a resolver with effect management.
    /// </summary>
    public SpecialActionResolver(IDiceRoller diceRoller, EffectManager effectManager)
    {
      _diceRoller = diceRoller;
      _effectManager = effectManager;
    }

    /// <summary>
    /// Resolves a special action against passive defense.
    /// </summary>
    public SpecialActionResult Resolve(SpecialActionRequest request)
    {
      int tv = request.GetPassiveDefenseTV();
      return ResolveWithTV(request, tv);
    }

    /// <summary>
    /// Resolves a special action with explicit TV (for active defense).
    /// </summary>
    public SpecialActionResult ResolveWithTV(SpecialActionRequest request, int tv)
    {
      // Validate knockback capability
      if (request.ActionType == SpecialActionType.Knockback && !request.WeaponHasKnockback)
      {
        throw new InvalidOperationException("Knockback requires a weapon with knockback capability.");
      }

      // Validate called shot target
      if (request.ActionType == SpecialActionType.CalledShot && request.TargetLocation == null)
      {
        throw new InvalidOperationException("Called Shot requires a target location.");
      }

      // Calculate attack
      int effectiveAS = request.GetEffectiveAS();
      int attackRoll = _diceRoller.Roll4dFPlus();
      int av = effectiveAS + attackRoll;
      int sv = av - tv;

      // Miss
      if (sv < 0)
      {
        return SpecialActionResult.Miss(request.ActionType, effectiveAS, attackRoll, av, tv, sv);
      }

      // Resolve by action type
      return request.ActionType switch
      {
        SpecialActionType.Knockback => ResolveKnockback(request, effectiveAS, attackRoll, av, tv, sv),
        SpecialActionType.Disarm => ResolveDisarm(request, effectiveAS, attackRoll, av, tv, sv),
        SpecialActionType.CalledShot => ResolveCalledShot(request, effectiveAS, attackRoll, av, tv, sv),
        SpecialActionType.StunningBlow => ResolveStunningBlow(request, effectiveAS, attackRoll, av, tv, sv),
        _ => throw new InvalidOperationException($"Unknown action type: {request.ActionType}")
      };
    }

    /// <summary>
    /// Resolves a special action and applies effects to the target character.
    /// </summary>
    public async Task<SpecialActionResult> ResolveAndApplyAsync(
      SpecialActionRequest request,
      int targetCharacterId)
    {
      var result = Resolve(request);

      if (result.Success && _effectManager != null)
      {
        await ApplyEffectsAsync(result, targetCharacterId);
      }

      return result;
    }

    /// <summary>
    /// Resolves a special action with explicit TV and applies effects.
    /// </summary>
    public async Task<SpecialActionResult> ResolveWithTVAndApplyAsync(
      SpecialActionRequest request,
      int tv,
      int targetCharacterId)
    {
      var result = ResolveWithTV(request, tv);

      if (result.Success && _effectManager != null)
      {
        await ApplyEffectsAsync(result, targetCharacterId);
      }

      return result;
    }

    private SpecialActionResult ResolveKnockback(
      SpecialActionRequest request,
      int effectiveAS, int attackRoll, int av, int tv, int sv)
    {
      double duration = KnockbackTable.GetDurationSeconds(sv);
      bool knockedProne = KnockbackTable.IsCritical(sv);

      return SpecialActionResult.KnockbackSuccess(
        effectiveAS, attackRoll, av, tv, sv, duration, knockedProne);
    }

    private SpecialActionResult ResolveDisarm(
      SpecialActionRequest request,
      int effectiveAS, int attackRoll, int av, int tv, int sv)
    {
      bool itemBroken = sv >= DisarmCriticalThreshold;
      string itemDesc = request.TargetItemDescription ?? "weapon";

      return SpecialActionResult.DisarmSuccess(
        effectiveAS, attackRoll, av, tv, sv, itemDesc, itemBroken);
    }

    private SpecialActionResult ResolveCalledShot(
      SpecialActionRequest request,
      int effectiveAS, int attackRoll, int av, int tv, int sv)
    {
      // Called shot with -2 penalty hit - check if it hit intended location
      // The -2 penalty is already in the AS, so if we hit (SV >= 0), we hit intended
      // But per design Option B: if called shot "misses" the location but attack succeeds,
      // hit random location. This happens at SV 0-1 (marginal success)
      bool hitIntendedLocation = sv >= 2; // Need decisive success for intended location
      HitLocation actualLocation;

      if (hitIntendedLocation)
      {
        actualLocation = request.TargetLocation!.Value;
      }
      else
      {
        // Marginal success - hit random location
        int locationRoll = _diceRoller.Roll(1, 24);
        actualLocation = HitLocationCalculator.MapRollToLocation(locationRoll);
      }

      // Roll Physicality bonus
      int physRoll = request.AttackerPhysicalityAS + _diceRoller.Roll4dFPlus();
      int physRV = physRoll - PhysicalityTV;
      var physBonus = CombatResultTables.GetPhysicalityBonus(physRV);

      int finalSV = sv + physBonus.SVModifier;
      var damage = CombatResultTables.GetDamage(finalSV);

      return SpecialActionResult.CalledShotSuccess(
        effectiveAS, attackRoll, av, tv, sv, actualLocation, hitIntendedLocation,
        physBonus, finalSV, damage);
    }

    private SpecialActionResult ResolveStunningBlow(
      SpecialActionRequest request,
      int effectiveAS, int attackRoll, int av, int tv, int sv)
    {
      // Stun duration = SV seconds
      int stunDuration = Math.Max(1, sv); // Minimum 1 second
      bool causedUnconscious = sv >= StunCriticalThreshold;

      return SpecialActionResult.StunningBlowSuccess(
        effectiveAS, attackRoll, av, tv, sv, stunDuration, causedUnconscious);
    }

    private async Task ApplyEffectsAsync(SpecialActionResult result, int targetCharacterId)
    {
      if (_effectManager == null) return;

      switch (result.ActionType)
      {
        case SpecialActionType.Knockback:
          // Knockback is immediate - target cannot act for duration
          // This is tracked in combat state, not as an Effect
          // If critical, apply Prone effect
          if (result.KnockedProne)
          {
            await _effectManager.ApplyEffectAsync(targetCharacterId, "Prone");
          }
          break;

        case SpecialActionType.StunningBlow:
          if (result.CausedUnconscious)
          {
            await _effectManager.ApplyEffectAsync(targetCharacterId, "Unconscious");
          }
          else if (result.StunDurationSeconds.HasValue)
          {
            // Apply Stunned effect - duration is in seconds
            // Convert to rounds (3 seconds per round, round up)
            int durationRounds = (int)Math.Ceiling(result.StunDurationSeconds.Value / 3.0);
            await _effectManager.ApplyEffectAsync(
              targetCharacterId,
              "Stunned",
              durationOverride: durationRounds);
          }
          break;

        case SpecialActionType.Disarm:
          // Disarm result is handled by combat state/game master
          // Mark weapon as dropped in character's equipment
          break;

        case SpecialActionType.CalledShot:
          // Damage/wounds applied through normal damage resolution
          // This result provides the location and damage for that process
          break;
      }
    }
  }
}
