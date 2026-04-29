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
        public Observable<ILiveOps> OnExpired => _onExpired;
        public Observable<LiveOpsRuntime> OnDataChanged => _onDataChanged;

        LiveOpsData ILiveOps.Data => _data;
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
        protected readonly Subject<ILiveOps> _onExpired = new();
        protected readonly Subject<LiveOpsRuntime> _onDataChanged = new();
        protected readonly CompositeDisposable _disposables = new();

        protected DiContainer _diContainer;
        protected IProgressService _progressService;
        protected IUIService _uiService;
        protected LiveOpsIcon _icon;

        protected ICondition[] _conditions;

        protected LiveOpsBase(DiContainer diContainer, TData data)
        {
            _diContainer = diContainer;
            _data = data;
            _progressService = diContainer.Resolve<IProgressService>();
            _uiService = diContainer.Resolve<IUIService>();

            _runtime = _progressService.GetLiveOpsValue<TRuntime>(Name) ?? new TRuntime();
            EndTime = _data.GetEndTime(_runtime);
            _conditions = CreateConditions();

            diContainer.Resolve<ISystemService>().OnTick
                .Subscribe(_ => OnTick())
                .AddTo(_disposables);
        }

        public LiveOpsIcon CreateIcon()
        {
            if (_data.IconPrefab == null)
                return null;
            _icon = Object.Instantiate(_data.IconPrefab);
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

        public bool IsExpired() => GetRemainingTime() <= TimeSpan.Zero;

        public TimeSpan GetRemainingTime()
        {
            var remaining = EndTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public virtual void Save()
        {
            _progressService.SetLiveOpsValue(_runtime, Name);
        }

        public void BroadcastUpdate() => _onDataChanged.OnNext(_runtime);

        public virtual void UpdatePrepare(bool debug = false) { }
        public virtual UniTask UpdateVisual(Transform from, bool debug = false) => UniTask.CompletedTask;
        public abstract UniTask UpdatePopups();

        public virtual void Dispose() => _disposables?.Dispose();

        protected abstract ICondition[] CreateConditions();
        protected bool CanActivate() => _conditions.All(condition => condition.Evaluate());

        private void OnTick()
        {
            if (_runtime.State == State.Active && IsExpired())
                _onExpired.OnNext(this);
        }
    }
}
