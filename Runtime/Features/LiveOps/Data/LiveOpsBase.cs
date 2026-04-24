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
    public abstract class LiveOpsBase<TData, TRuntime> : ILiveOps
        where TData : LiveOpsData
        where TRuntime : LiveOpsRuntime, new()
    {
        public Observable<ILiveOps> OnStarted => _onStarted;
        public Observable<ILiveOps> OnStage => _onStage;
        public Observable<ILiveOps> OnFinished => _onFinished;
        public Observable<LiveOpsRuntime> OnDataChanged => _onDataChanged;

        LiveOpsRuntime ILiveOps.Runtime => _runtime;
        public TRuntime Runtime => _runtime;
        public TData Data => _data;
        public string Name => _data.Name;
        public DateTime EndTime { get; protected set; }

        protected TData _data;
        protected TRuntime _runtime;

        protected readonly Subject<ILiveOps> _onStarted = new();
        protected readonly Subject<ILiveOps> _onStage = new();
        protected readonly Subject<ILiveOps> _onFinished = new();
        protected readonly Subject<LiveOpsRuntime> _onDataChanged = new();
        protected readonly CompositeDisposable _disposables = new();

        protected DiContainer _diContainer;
        protected IProgressService _progressService;
        protected IUIService _uiService;
        protected LiveOpsIcon _icon;

        private ICondition[] _conditions;

        public void Initialize(DiContainer diContainer, LiveOpsData data)
        {
            _diContainer = diContainer;
            _data = (TData)data;
            _progressService = diContainer.Resolve<IProgressService>();
            _uiService = diContainer.Resolve<IUIService>();

            _runtime = _progressService.GetLiveOpsValue<TRuntime>(Name) ?? new TRuntime();
            EndTime = _data.GetEndTime(_runtime);
            OnInitialize();
            _conditions = CreateConditions();
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

        public TimeSpan GetRemainingTime()
        {
            var remaining = EndTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public virtual void Save()
        {
            _progressService.SetLiveOpsValue(_runtime, Name);
        }

        public virtual void UpdatePrepare(bool debug = false) { }
        public abstract UniTask UpdateVisual(Transform from, bool debug = false);
        public abstract UniTask UpdatePopups();

        public virtual void Dispose() => _disposables?.Dispose();

        protected abstract void OnInitialize();
        protected abstract ICondition[] CreateConditions();
        protected bool CanActivate() => _conditions.All(condition => condition.Evaluate());
    }
}
