using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services;
using TapEmpire.UI;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.CoreSystems
{
    public abstract class UICoreSystem : Initializable, IUICoreSystem
    {
        public ReadOnlyReactiveProperty<bool> IsUIBlocked => _isUIBlocked;

        protected IUIService _uiService;
        protected CompositeDisposable _disposables = new();

        private ISceneContextsService _sceneContextsService;
        private ILevelExecutionCoreSystem _levelExecutionCoreSystem;
        private IInputCoreSystem _inputCoreSystem;

        private ReactiveProperty<bool> _isUIBlocked = new(false);
        private HashSet<IUIViewModel> _hudViewModels = new();

        private int _blockCounter;

        [Inject]
        private void Construct(ISceneContextsService sceneContextsService, IUIService uiService,
            ILevelExecutionCoreSystem levelExecutionCoreSystem, IInputCoreSystem inputCoreSystem)
        {
            _sceneContextsService = sceneContextsService;
            _uiService = uiService;
            _levelExecutionCoreSystem = levelExecutionCoreSystem;
            _inputCoreSystem = inputCoreSystem;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _sceneContextsService.OnSceneContextInstalledR3.Subscribe(OnSceneContextInstalled).AddTo(_disposables);
            _levelExecutionCoreSystem.ExecutionData.Subscribe(OnUpdateExecutionData).AddTo(_disposables);

            _uiService.OnBeforeOpenView += UIService_OnBeforeOpenView;
            _uiService.OnAfterCloseView += UIService_OnAfterCloseView;

            if (_uiService.TryGetModel<SceneLoadingUIViewModel>(out _))
            {
                BlockUI(true);
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _uiService.OnBeforeOpenView -= UIService_OnBeforeOpenView;
            _uiService.OnAfterCloseView -= UIService_OnAfterCloseView;

            _hudViewModels.ForEach(viewModel => _uiService.CloseViewAsync(viewModel, default).Forget());
            _hudViewModels.Clear();

            _disposables.Dispose();
        }

        public void BlockUI(bool shouldBlock)
        {
            _blockCounter += shouldBlock ? 1 : -1;
            _isUIBlocked.Value = _blockCounter > 0;
            _inputCoreSystem.BlockModeProperty.Value = _isUIBlocked.Value;
        }

        protected abstract UniTask CreateUIAsync(CancellationToken cancellationToken);

        protected async UniTask OpenHudViewAsync<T>(UIView viewPrefab, T viewModel, CancellationToken cancellationToken)
            where T : IUIViewModel
        {
            _hudViewModels.Add(viewModel);
            await _uiService.OpenViewAsync(viewPrefab, viewModel, cancellationToken);
        }

        protected virtual bool ShouldBlockFor(IUIViewModel viewModel)
        {
            return !_hudViewModels.Contains(viewModel);
        }

        private void OnSceneContextInstalled((string, SceneContext) eventData)
        {
            CreateUIAsync(default).Forget();
        }

        private void OnUpdateExecutionData(LevelExecutionData levelExecutionData)
        {
            BlockUI(levelExecutionData == null);
        }

        private void UIService_OnBeforeOpenView(IUIViewModel viewModel)
        {
            if (ShouldBlockFor(viewModel))
            {
                BlockUI(true);
                _levelExecutionCoreSystem.PauseLevel(true);
            }
        }

        private void UIService_OnAfterCloseView(IUIViewModel viewModel)
        {
            if (ShouldBlockFor(viewModel))
            {
                BlockUI(false);
                _levelExecutionCoreSystem.PauseLevel(false);
            }
        }
    }
}
