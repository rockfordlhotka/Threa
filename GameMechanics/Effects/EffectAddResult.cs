using System;
using System.Collections.Generic;

namespace GameMechanics.Effects;

/// <summary>
/// The outcome when attempting to add an effect to a character.
/// </summary>
public enum EffectAddOutcome
{
  /// <summary>
  /// Add this effect normally to the character.
  /// </summary>
  Add,

  /// <summary>
  /// Don't add this effect (e.g., character is immune, or it was merged into an existing effect).
  /// </summary>
  Reject,

  /// <summary>
  /// Replace an existing effect with this one.
  /// </summary>
  Replace,

  /// <summary>
  /// Add this effect, plus create additional effects (e.g., overdose side effect).
  /// </summary>
  AddWithSideEffects
}

/// <summary>
/// Result of attempting to add an effect to a character.
/// </summary>
public class EffectAddResult
{
  /// <summary>
  /// The outcome of the add attempt.
  /// </summary>
  public EffectAddOutcome Outcome { get; set; } = EffectAddOutcome.Add;

  /// <summary>
  /// If Outcome is Replace, this is the ID of the effect to replace.
  /// </summary>
  public Guid? ReplaceEffectId { get; set; }

  /// <summary>
  /// If Outcome is AddWithSideEffects, these are additional effects to create.
  /// </summary>
  public List<EffectCreateRequest> SideEffects { get; set; } = [];

  /// <summary>
  /// Optional message explaining the result (for logging/display).
  /// </summary>
  public string? Message { get; set; }

  /// <summary>
  /// Creates a result indicating the effect should be added normally.
  /// </summary>
  public static EffectAddResult AddNormally() => new() { Outcome = EffectAddOutcome.Add };

  /// <summary>
  /// Creates a result indicating the effect should be rejected.
  /// </summary>
  public static EffectAddResult Reject(string? message = null) => new()
  {
    Outcome = EffectAddOutcome.Reject,
    Message = message
  };

  /// <summary>
  /// Creates a result indicating an existing effect should be replaced.
  /// </summary>
  public static EffectAddResult Replace(Guid existingEffectId, string? message = null) => new()
  {
    Outcome = EffectAddOutcome.Replace,
    ReplaceEffectId = existingEffectId,
    Message = message
  };

  /// <summary>
  /// Creates a result indicating the effect should be added along with side effects.
  /// </summary>
  public static EffectAddResult AddWithSideEffects(List<EffectCreateRequest> sideEffects, string? message = null) => new()
  {
    Outcome = EffectAddOutcome.AddWithSideEffects,
    SideEffects = sideEffects,
    Message = message
  };
}

/// <summary>
/// Request to create an effect, used for side effects during add.
/// </summary>
public class EffectCreateRequest
{
  /// <summary>
  /// The effect type to create.
  /// </summary>
  public required Threa.Dal.Dto.EffectType EffectType { get; set; }

  /// <summary>
  /// Display name for the effect.
  /// </summary>
  public required string Name { get; set; }

  /// <summary>
  /// Optional body location for the effect.
  /// </summary>
  public string? Location { get; set; }

  /// <summary>
  /// Duration in rounds (null for permanent/until-removed).
  /// </summary>
  public int? DurationRounds { get; set; }

  /// <summary>
  /// Initial behavior state data.
  /// </summary>
  public string? BehaviorState { get; set; }
}
