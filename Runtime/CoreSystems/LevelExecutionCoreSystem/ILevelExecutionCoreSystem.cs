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

        Subject<LevelExecutionData> OnLevelStarted { get; }
        Subject<LevelEndReason> OnLevelCompleted { get; }
        Subject<int> OnCycleCompleted { get; }

        void StartLevel(int levelIndex);
        void PauseLevel(bool shouldPause);
        IDisposable PauseLevel();
        void Continue();
        void ExitLevel(LevelEndReason reason);
        void RestartLevel();
        void SetShouldSkipAd(bool shouldSkip);
    }
}