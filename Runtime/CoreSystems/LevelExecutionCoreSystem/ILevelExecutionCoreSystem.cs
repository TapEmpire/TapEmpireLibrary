using System;
using System.Collections.Generic;
using R3;
using TapEmpire.Level;

namespace TapEmpire.CoreSystems
{
    public interface ILevelExecutionCoreSystem : IExecutionCoreSystem
    {
        ReactiveProperty<LevelExecutionData> ExecutionData { get; }
        IReadOnlyList<LevelSettings> Levels { get; }

        Subject<LevelExecutionData> OnLevelStartedR3 { get; }
        Subject<LevelEndReason> OnLevelCompletedR3 { get; }

        event Action<LevelExecutionData> OnLevelStarted;
        event Action<LevelEndReason> OnLevelCompleted;
        event Action<int> OnCycleCompleted;

        void StartLevel(int levelIndex);
        void Continue();
        void ExitLevel(LevelEndReason reason);
        void RestartLevel(bool isDebug = false);
        void SetShouldSkipAd(bool shouldSkip);
    }
}