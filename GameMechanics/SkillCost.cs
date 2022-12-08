using System;

namespace GameMechanics
{
  public static class SkillCost
  {
    public static int GetBonus(int level)
    {
      if (level < -1 || level > 10)
        throw new ArgumentException(nameof(level));
      return level - 5;
    }

    public static double GetLevelUpCost(int startLevel, int difficulty)
    {
      if (startLevel < -1 || startLevel >= 10)
        throw new ArgumentException(nameof(startLevel));
      if (difficulty < 0 || difficulty > 14)
        throw new ArgumentException(nameof(difficulty));
      if (startLevel < 0)
        return 0;
      else
        return Costs[startLevel, difficulty - 1];
    }

    private static readonly double[,] Costs =
    {
      { 0.1, 0.3, 0.5, 1, 1, 1, 2, 2, 3, 4, 4, 5, 6, 6 },
      { 0.3, 0.5, 1, 2, 3, 3, 4, 5, 5, 6, 6, 7, 8, 8 },
      { 0.5, 1, 2, 3, 4, 5, 6, 7, 7, 8, 9, 10, 10, 11 },
      { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
      { 2, 3, 4, 5, 6, 8, 9, 10, 12, 13, 14, 16, 17, 18 },
      { 3, 5, 7, 10, 12, 15, 17, 20, 22, 24, 27, 29, 32, 34 },
      { 4, 7, 11, 14, 18, 21, 25, 28, 32, 35, 39, 43, 46, 50 },
      { 8, 17, 25, 33, 41, 50, 58, 66, 74, 83, 91, 99, 107, 116 },
      { 22, 44, 65, 87, 109, 131, 152, 174, 196, 218, 240, 261, 283, 305 },
      { 37, 74, 111, 148, 185, 222, 259, 296, 333, 370, 407, 444, 481, 518 }
    };
  }
}
