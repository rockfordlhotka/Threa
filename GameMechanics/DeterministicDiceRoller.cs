using System.Collections.Generic;

namespace GameMechanics;

/// <summary>
/// Deterministic dice roller for testing. Returns predetermined values.
/// </summary>
public class DeterministicDiceRoller : IDiceRoller
{
  private readonly Queue<int> _fudgeRolls = new();
  private readonly Queue<int> _diceRolls = new();
  private int _defaultFudgeResult = 0;
  private int _defaultDiceResult = 1;

  /// <summary>
  /// Creates a deterministic roller with no preset values.
  /// Will return defaults (0 for fudge, 1 for standard dice).
  /// </summary>
  public DeterministicDiceRoller() { }

  /// <summary>
  /// Creates a deterministic roller that returns a fixed 4dF+ result.
  /// </summary>
  /// <param name="fixed4dFPlusResult">The result to return for Roll4dFPlus().</param>
  public static DeterministicDiceRoller WithFixed4dFPlus(int fixed4dFPlusResult)
  {
    var roller = new DeterministicDiceRoller();
    // Queue individual fudge rolls to achieve the target sum
    // For simplicity, we'll use a special flag approach
    roller._fixed4dFPlusResult = fixed4dFPlusResult;
    roller._useFixed4dFPlus = true;
    return roller;
  }

  private int? _fixed4dFPlusResult;
  private bool _useFixed4dFPlus;

  /// <summary>
  /// Queues a sequence of 4dF+ results to return in order.
  /// </summary>
  public DeterministicDiceRoller Queue4dFPlusResults(params int[] results)
  {
    foreach (var r in results)
      _queue4dFPlus.Enqueue(r);
    return this;
  }

  private readonly Queue<int> _queue4dFPlus = new();

  /// <summary>
  /// Queues individual fudge die results (-1, 0, or +1).
  /// </summary>
  public DeterministicDiceRoller QueueFudgeRolls(params int[] results)
  {
    foreach (var r in results)
      _fudgeRolls.Enqueue(r);
    return this;
  }

  /// <summary>
  /// Queues standard dice results.
  /// </summary>
  public DeterministicDiceRoller QueueDiceRolls(params int[] results)
  {
    foreach (var r in results)
      _diceRolls.Enqueue(r);
    return this;
  }

  /// <summary>
  /// Sets the default result when queue is empty.
  /// </summary>
  public DeterministicDiceRoller SetDefaults(int fudgeResult = 0, int diceResult = 1)
  {
    _defaultFudgeResult = fudgeResult;
    _defaultDiceResult = diceResult;
    return this;
  }

  public int Roll(int count, int size)
  {
    int result = 0;
    for (int i = 0; i < count; i++)
    {
      result += _diceRolls.Count > 0 ? _diceRolls.Dequeue() : _defaultDiceResult;
    }
    return result;
  }

  public int RollFudge()
  {
    return _fudgeRolls.Count > 0 ? _fudgeRolls.Dequeue() : _defaultFudgeResult;
  }

  public int Roll4dFPlus()
  {
    // If we have queued 4dF+ results, use those
    if (_queue4dFPlus.Count > 0)
      return _queue4dFPlus.Dequeue();

    // If a fixed result was set, return that
    if (_useFixed4dFPlus && _fixed4dFPlusResult.HasValue)
      return _fixed4dFPlusResult.Value;

    // Otherwise, simulate using fudge rolls
    int result = 0;
    for (int i = 0; i < 4; i++)
      result += RollFudge();

    // Note: explosion logic not simulated in deterministic mode
    // to keep tests predictable
    return result;
  }
}
