using System;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Tracks aim state for a character.
  /// Aiming provides +2 AS if the first action next round is a ranged attack on the same target.
  /// </summary>
  public class AimState
  {
    /// <summary>
    /// The ID of the target being aimed at.
    /// </summary>
    public string? TargetId { get; private set; }

    /// <summary>
    /// The round in which aiming started.
    /// </summary>
    public int AimRound { get; private set; }

    /// <summary>
    /// Whether the aim is currently active.
    /// </summary>
    public bool IsAiming => TargetId != null;

    /// <summary>
    /// The aim bonus to apply (+2 AS).
    /// </summary>
    public const int AimBonus = 2;

    /// <summary>
    /// Starts aiming at a target.
    /// </summary>
    /// <param name="targetId">The ID of the target.</param>
    /// <param name="currentRound">The current round number.</param>
    public void StartAiming(string targetId, int currentRound)
    {
      if (string.IsNullOrEmpty(targetId))
        throw new ArgumentNullException(nameof(targetId));

      TargetId = targetId;
      AimRound = currentRound;
    }

    /// <summary>
    /// Checks if the aim bonus applies for a ranged attack.
    /// The bonus applies if:
    /// - This is the first action of the round following the aim
    /// - The target is the same as the aimed target
    /// </summary>
    /// <param name="targetId">The target of the attack.</param>
    /// <param name="currentRound">The current round number.</param>
    /// <param name="actionsThisRound">Number of actions already taken this round.</param>
    /// <returns>True if the aim bonus applies.</returns>
    public bool CheckAimBonus(string targetId, int currentRound, int actionsThisRound)
    {
      if (!IsAiming)
        return false;

      // Must be the round after aiming
      if (currentRound != AimRound + 1)
        return false;

      // Must be the first action
      if (actionsThisRound > 0)
        return false;

      // Must be the same target
      if (targetId != TargetId)
        return false;

      return true;
    }

    /// <summary>
    /// Consumes the aim (call after a successful aim bonus application or when aim is lost).
    /// </summary>
    public void ConsumeAim()
    {
      TargetId = null;
      AimRound = 0;
    }

    /// <summary>
    /// Loses the aim due to interruption.
    /// Called when:
    /// - Taking damage
    /// - Target moves significantly
    /// - Taking any non-ranged-attack action
    /// </summary>
    public void LoseAim()
    {
      ConsumeAim();
    }

    /// <summary>
    /// Checks and applies aim bonus to a ranged attack request.
    /// </summary>
    /// <param name="request">The ranged attack request.</param>
    /// <param name="targetId">The target ID.</param>
    /// <param name="currentRound">The current round.</param>
    /// <returns>True if aim bonus was applied.</returns>
    public bool TryApplyAimBonus(RangedAttackRequest request, string targetId, int currentRound)
    {
      if (CheckAimBonus(targetId, currentRound, request.ActionsThisRound))
      {
        // Note: The request should already have HasAimBonus set if applicable
        // This method is for validation/tracking
        ConsumeAim();
        return true;
      }

      return false;
    }
  }
}
