using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Patterns.Strategy;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.LiveOps
{
    public class LiveOpsService : Initializable, ILiveOpsService
    {
        [field: SerializeField] public LiveOpsSettings Settings { get; private set; }

        private DiContainer _diContainer;
        private IProgressService _progressService;
        private IUIService _uiService;
        
        private List<ILiveOps> _liveOps = new();
        private readonly Dictionary<Type, IHandler> _handlers = new();
        private CompositeDisposable _disposables = new();
        private Transform _resourceEmitter = null;

        [Inject]
        private void Construct(DiContainer diContainer, IProgressService progressService, IUIService uiService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _uiService = uiService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();
            _liveOps = new();

            new LiveOpsAnalyticsModule(_diContainer).AddTo(_disposables);

            Settings.LiveOps.ForEach(liveOpsData => InitializeAndRegisterLiveOps(liveOpsData));

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _liveOps.Clear();
            _disposables.Dispose();
            base.OnRelease();
        }

        public T GetLiveOps<T>() where T : ILiveOps
        {
            return _liveOps.OfType<T>().FirstOrDefault();
        }

        public List<ILiveOps> GetLiveOps() => _liveOps;

        public async UniTask UpdateLiveOps()
        {
            await _liveOps.Select(liveOps => liveOps.UpdateVisual(_resourceEmitter));

            foreach (var liveOps in _liveOps)
            {
                await liveOps.UpdateState();
            }
        }

        private void InitializeAndRegisterLiveOps(LiveOpsData data)
        {
            var liveOps = data.Create();
            liveOps.Initialize(_diContainer, data);
            liveOps.AddTo(_disposables);
            _liveOps.Add(liveOps);
        }
    }
}
