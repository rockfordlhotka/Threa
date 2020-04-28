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
      {
        var r = Roll(size);
        result = result + r;
      }

      if (count == 4 && size == 3)
      {
        if (result == 4)
          result += Roll4d3Bonus();
        else if (result == -4)
          result -= Roll4d3Bonus();
      }
      return result;
    }

    private static int Roll(int size)
    {
      if (size == 3)
        return _rnd.Next(-1, 2);
      else
        return _rnd.Next(1, size + 1);
    }

    public static int Roll4d3Bonus()
    {
      int result = 0;

      for (int i = 0; i < 4; i++)
        if (Roll(3) > 0)
          result++;

      if (result == 4)
        result += Roll4d3Bonus();

      return result;
    }
  }
}