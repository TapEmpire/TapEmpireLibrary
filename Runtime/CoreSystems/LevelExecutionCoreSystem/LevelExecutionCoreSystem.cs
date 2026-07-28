using System;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Level;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.CoreSystems
{
    [Serializable]
    public class LevelExecutionCoreSystem : ExecutionCoreSystem
    {
        [SerializeField] private LevelView _defaultLevelView = null;

        public ReactiveProperty<LevelExecutionData> ExecutionData { get; } = new();

        protected DiContainer _diContainer;
        protected ICoreSceneReferences _coreSceneReferences;

        protected CompositeDisposable _disposables = new();
        protected IExecutionAction<FlowAction> _executionAction = null;

        protected override IExecutionModule[] ExecutionModules
            => ExecutionData.Value?.LevelView.LevelModules ?? Array.Empty<IExecutionModule>();

        [Inject]
        private void Construct(DiContainer diContainer, ICoreSceneReferences coreSceneReferences)
        {
            _diContainer = diContainer;
            _coreSceneReferences = coreSceneReferences;
        }

        protected override void OnRelease()
        {
            DestroyLevel();
        }

        protected virtual void DestroyLevel()
        {
            if (ExecutionData.Value != null)
            {
                ExecutionData.Value.Release();
                ExecutionData.Value = null;
            }

            _disposables.Dispose();
            _executionAction?.Dispose();
        }

        protected async UniTask InitializeLevelView(LevelSettings levelSettings, int levelIndex)
        {
            var levelView = await InstantiateLevelView(levelSettings);

            ExecutionData.Value = new LevelExecutionData(levelSettings, levelView, levelIndex);
            ExecutionData.Value.LevelStateData.OnDataChanged.Subscribe(OnLevelStateChanged).AddTo(_disposables);

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
