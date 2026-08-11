using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Level;
using TapEmpire.Messages;
using TapEmpire.Services;
using TapEmpire.Settings;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class LevelExecutionCoreSystem : ExecutionCoreSystem, ILevelExecutionCoreSystem
    {
        private const float LoadingScreenCloseDelay = 0.1f;

        [SerializeField] private LevelView _defaultLevelView = null;

        public Subject<LevelExecutionData> OnLevelStarted { get; } = new();
        public Subject<LevelEndReason> OnLevelCompleted { get; } = new();
        public Subject<int> OnCycleCompleted { get; } = new();

        public ReactiveProperty<LevelExecutionData> ExecutionData { get; } = new();

        public IReadOnlyList<LevelSettings> Levels => _gameService.LevelsTable.Levels;

        protected DiContainer _diContainer;
        protected ICoreSceneReferences _coreSceneReferences;
        protected IGameService _gameService;
        protected IProgressService _progressService;
        protected IAdsService _adsService;
        protected INetworkService _networkService;
        protected ISceneManagementService _sceneManagementService;
        protected CoreSystemsContainer _coreSystemsContainer;
        protected ITicksContainer _ticksContainer;

        protected GameSettings GameSettings => _gameService.GameSettings;

        protected CompositeDisposable _disposables = new();
        protected IExecutionAction<FlowAction> _executionAction = null;
        protected bool _shouldSkipAd;
        protected int _pauseCounter;

        protected override IExecutionModule[] ExecutionModules
            => ExecutionData.Value?.LevelView.LevelModules ?? Array.Empty<IExecutionModule>();

        [Inject]
        private void Construct(DiContainer diContainer, ICoreSceneReferences coreSceneReferences,
            IGameService gameService, IProgressService progressService, IAdsService adsService,
            INetworkService networkService, ISceneManagementService sceneManagementService,
            CoreSystemsContainer coreSystemsContainer, ITicksContainer ticksContainer)
        {
            _ticksContainer = ticksContainer;
            _diContainer = diContainer;
            _coreSceneReferences = coreSceneReferences;
            _gameService = gameService;
            _progressService = progressService;
            _adsService = adsService;
            _networkService = networkService;
            _sceneManagementService = sceneManagementService;
            _coreSystemsContainer = coreSystemsContainer;
        }

        protected override void OnRelease()
        {
            DestroyLevel();
        }

        public virtual void Continue()
        {
            ProcessLevel(FlowAction.Next, LevelEndReason.Win);
        }

        public void RestartLevel()
        {
            OnLevelCompleted.OnNext(LevelEndReason.Retry);
            ProcessLevel(FlowAction.Restart, LevelEndReason.Retry);
        }

        public void ExitLevel(LevelEndReason reason)
        {
            OnLevelCompleted.OnNext(reason);
            ProcessLevel(FlowAction.Quit, reason);
        }

        public void StartLevel(int levelIndex)
        {
            var levels = _gameService.LevelsTable.Levels;
            levelIndex = MathUtility.LoopClamp(levelIndex, levels.Count);
            var level = levels[levelIndex];

            if (level != null)
            {
                StartLevel(level, levelIndex).Forget();
            }
        }

        public void PauseLevel(bool shouldPause)
        {
            _pauseCounter += shouldPause ? 1 : -1;

            ApplyPause();
        }

        public void SetShouldSkipAd(bool shouldSkip)
        {
            _shouldSkipAd = shouldSkip;
        }

        private void ApplyTicksPause()
        {
            _ticksContainer.IsPaused = ExecutionData.Value.LevelStateData.LevelState != LevelState.Active;
        }

        private void ApplyPause()
        {
            var levelStateData = ExecutionData.Value?.LevelStateData;
            if (levelStateData == null) return;

            if (_pauseCounter > 0 && levelStateData.LevelState == LevelState.Active)
            {
                levelStateData.SetState(LevelState.Pause);
            }
            else if (_pauseCounter <= 0 && levelStateData.LevelState == LevelState.Pause)
            {
                levelStateData.SetState(LevelState.Active);
            }
        }

        protected virtual void ProcessLevel(FlowAction flow, LevelEndReason reason)
        {
            if (flow == FlowAction.Next && GameSettings.AutoRestartLevel)
            {
                flow = FlowAction.Restart;
            }

            var shouldSkipAd = _shouldSkipAd || !AreAdsAllowed(flow);

            _executionAction = ExecutionAction<FlowAction>
                .Composite(
                    new NetworkExecutionAction(_networkService),
                    new AdsExecutionAction(_adsService, ExecutionData.Value.LevelIndex, shouldSkipAd),
                    new CallbackExecutionAction<FlowAction>(nextFlow => FinalizeFlow(nextFlow, reason)))
                .RunExecute(flow);
        }

        protected virtual void FinalizeFlow(FlowAction flow, LevelEndReason reason)
        {
            flow = flow == FlowAction.Next ? ConvertNextAction() : flow;

            switch (flow)
            {
                case FlowAction.Quit:
                    LoadScene(SceneName.Menu).Forget();
                    break;

                case FlowAction.Restart:
                    _progressService.CleanLevelSaveData();
                    StartLevel(this.GetLevelIndex());
                    break;

                case FlowAction.Next:
                    _progressService.SetVisualProgress((this.GetLevelIndex() + 1).ToString());
                    StartLevel(this.GetNextLevelIndex());
                    break;
            }
        }

        protected bool AreAdsAllowed(FlowAction flow)
        {
            return flow switch
            {
                FlowAction.Quit => GameSettings.AdsOnQuit,
                FlowAction.Restart => GameSettings.AdsOnRestart,
                _ => true,
            };
        }

        protected FlowAction ConvertNextAction()
        {
            var completedLevel = ExecutionData.Value.LevelIndex + 1;
            var winFlow = GameSettings.WinFlow;

            if (GameSettings.WinFlowExceptionLevels.Contains(completedLevel))
            {
                winFlow = winFlow == WinFlow.Next ? WinFlow.Menu : WinFlow.Next;
            }

            return winFlow == WinFlow.Next ? FlowAction.Next : FlowAction.Quit;
        }

        protected async UniTask LoadScene(SceneName sceneName)
        {
            await _sceneManagementService.CreateLoadingScreen(default, false);
            _coreSystemsContainer.Release();
            _sceneManagementService.LoadSceneAsync(sceneName, default, false, false).Forget();
        }

        protected virtual async UniTaskVoid StartLevel(LevelSettings level, int levelIndex)
        {
            DestroyLevel();

            _disposables = new();
            _shouldSkipAd = false;

            await InitializeLevelView(level, levelIndex);

            MessagesUtility.Invoke(MessageType.StartLevel, new StartLevelMessageData { LevelIndex = levelIndex });
            OnLevelStarted.OnNext(ExecutionData.Value);
            _progressService.SetLevelProgress(ExecutionData.Value.LevelIndex);
            _progressService.SetVisualProgress((ExecutionData.Value.LevelIndex + 1).ToString());

            _adsService.ShowBanner(true);

            // TODO revisit: fire and forget with no cancellation token.
            UniTaskUtility.ExecuteAfterSeconds(LoadingScreenCloseDelay,
                () => _sceneManagementService.CloseLoadingScreen(default), default).Forget();
        }

        protected virtual void DestroyLevel()
        {
            if (ExecutionData.Value != null)
            {
                ExecutionData.Value.Release();
                ExecutionData.Value = null;
            }

            _ticksContainer.IsPaused = false;

            _disposables.Dispose();
            _executionAction?.Dispose();
        }

        protected async UniTask InitializeLevelView(LevelSettings levelSettings, int levelIndex)
        {
            var levelView = await InstantiateLevelView(levelSettings);

            ExecutionData.Value = new LevelExecutionData(levelSettings, levelView, levelIndex);
            ExecutionData.Value.LevelStateData.OnDataChanged.Subscribe(OnLevelStateChanged).AddTo(_disposables);
            ExecutionData.Value.LevelStateData.OnDataChanged.Subscribe(_ => ApplyTicksPause()).AddTo(_disposables);

            ApplyPause();
            ApplyTicksPause();

            InitializeModules();
        }

        protected virtual void OnLevelStateChanged(LevelStateData levelStateData)
        {
        }

        protected async UniTask<LevelView> InstantiateLevelView(LevelSettings levelSettings)
        {
            if (levelSettings.LevelViewPrefab != null && levelSettings.LevelViewPrefab.RuntimeKeyIsValid())
            {
                var prefabInstance = await levelSettings.LevelViewPrefab.InstantiateAsync(_coreSceneReferences.LevelParent);
                return prefabInstance.GetComponent<LevelView>();
            }

            return UnityEngine.Object.Instantiate(_defaultLevelView, _coreSceneReferences.LevelParent);
        }

        protected void InitializeModules()
        {
            InitializeModules(_diContainer);
        }
    }
}
