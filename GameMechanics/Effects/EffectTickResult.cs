namespace GameMechanics.Effects;

/// <summary>
/// Result of an effect's OnTick execution.
/// </summary>
public class EffectTickResult
{
  /// <summary>
  /// Whether the effect should expire early (before its normal duration ends).
  /// </summary>
  public bool ShouldExpireEarly { get; set; }

  /// <summary>
  /// Optional message describing what happened during the tick.
  /// </summary>
  public string? Message { get; set; }

  /// <summary>
  /// Creates a result indicating normal tick completion.
  /// </summary>
  public static EffectTickResult Continue() => new();

  /// <summary>
  /// Creates a result indicating the effect should expire early.
  /// </summary>
  public static EffectTickResult ExpireEarly(string? message = null) => new()
  {
    ShouldExpireEarly = true,
    Message = message
  };
}
