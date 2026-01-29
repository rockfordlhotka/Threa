namespace GameMechanics.Effects;

/// <summary>
/// Result of an effect's OnTick execution.
/// </summary>
public class EffectTickResult
{
  /// <summary>
  /// Whether the effect should expire early (before its normal duration ends).
  /// When true, behavior depends on ExpireAsComplete:
  /// - ExpireAsComplete = true: calls OnExpire (successful completion)
  /// - ExpireAsComplete = false: calls OnRemove (interruption/removal)
  /// </summary>
  public bool ShouldExpireEarly { get; set; }

  /// <summary>
  /// When expiring early, whether this should be treated as a successful completion (true)
  /// or as an interruption/removal (false). Only relevant when ShouldExpireEarly is true.
  /// </summary>
  public bool ExpireAsComplete { get; set; }

  /// <summary>
  /// Optional message describing what happened during the tick.
  /// </summary>
  public string? Message { get; set; }

  /// <summary>
  /// Creates a result indicating normal tick completion.
  /// </summary>
  public static EffectTickResult Continue() => new();

  /// <summary>
  /// Creates a result indicating the effect should expire early and be removed (OnRemove called).
  /// Use this for effects that are interrupted, broken, or need to be removed prematurely.
  /// </summary>
  public static EffectTickResult ExpireEarly(string? message = null) => new()
  {
    ShouldExpireEarly = true,
    ExpireAsComplete = false,
    Message = message
  };

  /// <summary>
  /// Creates a result indicating the effect should expire early but be treated as a successful completion (OnExpire called).
  /// Use this for effects that reach their goal before their natural duration (e.g., concentration completing early).
  /// </summary>
  public static EffectTickResult CompleteEarly(string? message = null) => new()
  {
    ShouldExpireEarly = true,
    ExpireAsComplete = true,
    Message = message
  };
}
