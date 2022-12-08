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

    public static int Roll4dFWithBonus()
    {
      int result = Roll(4, "F");

      if (result == 4)
        result += Get4dFBonus();
      else if (result == -4)
        result -= Get4dFBonus();

      return result;
    }

    private static int Get4dFBonus()
    {
      var result = 0;
      for (int i = 0; i < 4; i++)
        if (RollF() > 0)
          result++;
      if (result == 4)
        result += Get4dFBonus();
      return result;
    }
  }
}