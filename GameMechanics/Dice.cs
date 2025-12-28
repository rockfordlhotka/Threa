using System;

namespace GameMechanics
{
  public static class Dice
  {
    private static Random _rnd;

    static Dice()
    {
      _rnd = new Random();
    }

    public static int Roll(int count, int size)
    {
      int result = 0;
      for (int i = 0; i < count; i++)
        result += Roll(size);
      return result;
    }

    public static int Roll(int count, string type)
    {
      if (type.ToUpper() != "F")
        throw new ArgumentException(nameof(type));
      int result = 0;
      for (int i = 0; i < count; i++)
        result += RollF();
      return result;
    }

    private static int Roll(int size)
    {
      return _rnd.Next(1, size + 1);
    }

    private static int RollF()
    {
      return _rnd.Next(-1, 2);
    }

    /// <summary>
    /// Rolls 4dF+ (exploding Fudge dice).
    /// On +4: Roll again, count only "+" results, add to total. Recurse if another +4.
    /// On -4: Roll again, count only "-" results, subtract from total. Recurse if another -4.
    /// </summary>
    public static int Roll4dFPlus()
    {
      int result = Roll(4, "F");

      if (result == 4)
        result += Get4dFExplosionBonus();
      else if (result == -4)
        result -= Get4dFExplosionPenalty();

      return result;
    }

    /// <summary>
    /// Rolls 4dF+ (exploding Fudge dice). Alias for Roll4dFPlus().
    /// </summary>
    [Obsolete("Use Roll4dFPlus() instead for clarity with spec notation.")]
    public static int Roll4dFWithBonus() => Roll4dFPlus();

    /// <summary>
    /// For +4 explosion: Roll 4dF, count only "+" results.
    /// If all 4 are "+", recurse (another +4 achieved).
    /// </summary>
    private static int Get4dFExplosionBonus()
    {
      var result = 0;
      for (int i = 0; i < 4; i++)
        if (RollF() > 0)
          result++;
      if (result == 4)
        result += Get4dFExplosionBonus();
      return result;
    }

    /// <summary>
    /// For -4 explosion: Roll 4dF, count only "-" results.
    /// If all 4 are "-", recurse (another -4 achieved).
    /// </summary>
    private static int Get4dFExplosionPenalty()
    {
      var result = 0;
      for (int i = 0; i < 4; i++)
        if (RollF() < 0)
          result++;
      if (result == 4)
        result += Get4dFExplosionPenalty();
      return result;
    }
  }
}