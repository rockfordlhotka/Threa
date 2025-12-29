namespace GameMechanics;

/// <summary>
/// Abstraction for dice rolling, enabling dependency injection and testability.
/// </summary>
public interface IDiceRoller
{
  /// <summary>
  /// Rolls the specified number of dice of the given size.
  /// </summary>
  /// <param name="count">Number of dice to roll.</param>
  /// <param name="size">Number of sides per die.</param>
  /// <returns>The sum of all dice.</returns>
  int Roll(int count, int size);

  /// <summary>
  /// Rolls 4dF+ (exploding Fudge dice).
  /// On +4: Roll again, count only "+" results, add to total. Recurse if another +4.
  /// On -4: Roll again, count only "-" results, subtract from total. Recurse if another -4.
  /// </summary>
  /// <returns>The result of the 4dF+ roll.</returns>
  int Roll4dFPlus();

  /// <summary>
  /// Rolls a single Fudge die (-1, 0, or +1).
  /// </summary>
  /// <returns>-1, 0, or +1</returns>
  int RollFudge();
}
