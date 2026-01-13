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

        public static int GetLevelUpCost(int startLevel, int difficulty)
        {
            if (startLevel < -1 || startLevel >= 10)
                throw new ArgumentException(nameof(startLevel));
            if (difficulty < 1 || difficulty > 14)
                throw new ArgumentException(nameof(difficulty));
            if (startLevel < 0)
                return 0;
            return Costs[startLevel, difficulty - 1];
        }

        /// <summary>
        /// Gets the cumulative XP cost to reach a given level from level 0.
        /// </summary>
        public static int GetCumulativeCost(int targetLevel, int difficulty)
        {
            int total = 0;
            for (int level = 0; level < targetLevel; level++)
            {
                total += GetLevelUpCost(level, difficulty);
            }
            return total;
        }

        private static readonly int[,] Costs =
        {
            { 1, 3, 5, 10, 10, 10, 20, 20, 30, 40, 40, 50, 60, 60 },
            { 3, 5, 10, 20, 30, 30, 40, 50, 50, 60, 60, 70, 80, 80 },
            { 5, 10, 20, 30, 40, 50, 60, 70, 70, 80, 90, 100, 100, 110 },
            { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140 },
            { 20, 30, 40, 50, 60, 80, 90, 100, 120, 130, 140, 160, 170, 180 },
            { 30, 50, 70, 100, 120, 150, 170, 200, 220, 240, 270, 290, 320, 340 },
            { 40, 70, 110, 140, 180, 210, 250, 280, 320, 350, 390, 430, 460, 500 },
            { 80, 170, 250, 330, 410, 500, 580, 660, 740, 830, 910, 990, 1070, 1160 },
            { 220, 440, 650, 870, 1090, 1310, 1520, 1740, 1960, 2180, 2400, 2610, 2830, 3050 },
            { 370, 740, 1110, 1480, 1850, 2220, 2590, 2960, 3330, 3700, 4070, 4440, 4810, 5180 }
        };
    }
}
