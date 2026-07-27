using UnityEngine;
using TapEmpire.Utility;

namespace TapEmpire.CoreSystems
{
    public static class LevelExecutionCoreSystemExtensions
    {
        public static bool TryGetCamera(this ILevelExecutionCoreSystem self, out Camera camera)
        {
            if (self.ExecutionData.Value == null || self.ExecutionData.Value.Camera == null)
            {
                camera = null;
                return false;
            }
            camera = self.ExecutionData.Value.Camera;
            return true;
        }

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

        public static void StartNextLevel(this ILevelExecutionCoreSystem self)
        {
            self.StartLevel(self.GetNextLevelIndex());
        }
    }
}