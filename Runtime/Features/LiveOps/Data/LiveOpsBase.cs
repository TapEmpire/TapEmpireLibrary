using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Feature.Tutorial;
using TapEmpire.LiveOps.UI;
using TapEmpire.UI;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace TapEmpire.Services.LiveOps
{
    public abstract class LiveOpsBase : ILiveOps
    {
        public Observable<ILiveOps> OnStarted => _onStarted;
        public Observable<ILiveOps> OnStage => _onStage;
        public Observable<ILiveOps> OnFinished => _onFinished;
        public Observable<StateData> OnDataChanged => _onDataChanged;

        public abstract StateData StateData { get; }
        public string Name => _data.Name;

        protected readonly Subject<ILiveOps> _onStarted = new();
        protected readonly Subject<ILiveOps> _onStage = new();
        protected readonly Subject<ILiveOps> _onFinished = new();
        protected readonly Subject<StateData> _onDataChanged = new();
        protected readonly CompositeDisposable _disposables = new();

        protected LiveOpsData _data;
        protected DiContainer _diContainer;
        protected IProgressService _progressService;
        protected IUIService _uiService;
        protected LiveOpsIcon _icon;
        protected ICondition[] _conditions;

        public void Initialize(DiContainer diContainer, LiveOpsData data)
        {
            _diContainer = diContainer;
            _data = data;
            _progressService = diContainer.Resolve<IProgressService>();
            _uiService = diContainer.Resolve<IUIService>();

            OnInitialize();
        }

        public IDisposable CreateIcon(Transform parent)
        {
            _icon = Object.Instantiate(_data.IconPrefab, parent);
            _diContainer.Inject(_icon);
            _icon.Initialize(this);
            return _icon;
        }

        public UniTask OpenView()
        {
            return _uiService.OpenViewAwaitable(_data.LiveOpsPrefab, new LiveOpsViewModel(this), default);
        }

        public UniTask OpenTutorial(bool isSkippable = true)
        {
            return _uiService.OpenViewAwaitable(_data.TutorialPrefab, new TutorialUIViewModel(Name, isSkippable), default);
        }

        public abstract TimeSpan GetRemainingTime();
        public abstract void Save();
        public virtual void UpdatePrepare(bool debug = false) { }
        public abstract UniTask UpdateVisual(Transform from, bool debug = false);
        public abstract UniTask UpdatePopups();

        public void Dispose() => _disposables?.Dispose();

        protected abstract void OnInitialize();
        protected bool CanActivate() => _conditions.All(condition => condition.Evaluate());
    }
}
