using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public static class LevelExecutionCoreSystemExtensions
    {
        public static int GetLevelIndex(this ILevelExecutionCoreSystem self)
        {
            return self.ExecutionData.Value.LevelIndex;
        }

        public static int GetNextLevelIndex(this ILevelExecutionCoreSystem self)
        {
            var levels = self.Levels;
            var currentLevelIndex = self.GetLevelIndex();

            return MathUtility.LoopClamp(currentLevelIndex + 1, levels.Count);
        }

        public static int GetPreviousLevelIndex(this ILevelExecutionCoreSystem self)
        {
            var levels = self.Levels;
            var currentLevelIndex = self.GetLevelIndex();

            return MathUtility.LoopValueBack(currentLevelIndex, levels.Count);
        }

        public static void StartNextLevel(this ILevelExecutionCoreSystem self)
        {
            self.StartLevel(self.GetNextLevelIndex());
        }

        public static void StartPreviousLevel(this ILevelExecutionCoreSystem self)
        {
            self.StartLevel(self.GetPreviousLevelIndex());
        }
    }
}