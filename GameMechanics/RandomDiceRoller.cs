using System;

namespace GameMechanics;

/// <summary>
/// Production dice roller using random number generation.
/// </summary>
public class RandomDiceRoller : IDiceRoller
{
  private readonly Random _rnd;

  public RandomDiceRoller()
  {
    _rnd = new Random();
  }

  public RandomDiceRoller(int seed)
  {
    _rnd = new Random(seed);
  }

  public int Roll(int count, int size)
  {
    int result = 0;
    for (int i = 0; i < count; i++)
      result += _rnd.Next(1, size + 1);
    return result;
  }

  public int RollFudge()
  {
    return _rnd.Next(-1, 2);
  }

  public int Roll4dFPlus()
  {
    int result = 0;
    for (int i = 0; i < 4; i++)
      result += RollFudge();

    if (result == 4)
      result += Get4dFExplosionBonus();
    else if (result == -4)
      result -= Get4dFExplosionPenalty();

    return result;
  }

  private int Get4dFExplosionBonus()
  {
    int result = 0;
    for (int i = 0; i < 4; i++)
      if (RollFudge() > 0)
        result++;
    if (result == 4)
      result += Get4dFExplosionBonus();
    return result;
  }

  private int Get4dFExplosionPenalty()
  {
    int result = 0;
    for (int i = 0; i < 4; i++)
      if (RollFudge() < 0)
        result++;
    if (result == 4)
      result += Get4dFExplosionPenalty();
    return result;
  }
}
