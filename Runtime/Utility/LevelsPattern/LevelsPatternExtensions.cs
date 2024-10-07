using System;
using System.Collections.Generic;
using System.Linq;

namespace TapEmpireLibrary.Utility
{
    public static class LevelsPatternExtensions
    {
        public static List<int> GetLevels(this LevelsPattern pattern, int totalLevels)
        {
            return pattern switch
            {
                LevelsPattern.Pattern_4_8_11_Plus3 => GeneratePattern_4_8_11_Plus3(totalLevels),
                LevelsPattern.Pattern_Each5 => GeneratePattern_Each(totalLevels, 5),
                LevelsPattern.Pattern_Each10 => GeneratePattern_Each(totalLevels, 10),
                _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
            };
        }
        
        private static List<int> GeneratePattern_4_8_11_Plus3(int totalLevels)
        {
            var levels = new List<int> { 4, 8, 11 };
            var nextValue = 11;

            while (nextValue + 3 < totalLevels)
            {
                nextValue += 3;
                levels.Add(nextValue);
            }

            return levels.Where(index => index < totalLevels).ToList();
        }

        private static List<int> GeneratePattern_Each(int totalLevels, int each)
        {
            var levels = new List<int>();
            var nextValue = each;

            while (nextValue < totalLevels)
            {
                levels.Add(nextValue);
                nextValue += each;
            }

            return levels;
        }
    }
}