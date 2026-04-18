using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Patterns.Strategy;
using TapEmpire.UI;
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

            Settings.LiveOps.ForEach(InitializeAndRegisterLiveOps);
            
            foreach (var handler in Settings.ConditionHandlers)
            {
                handler.Initialize(_diContainer);
                _handlers[handler.GetSubjectType()] = handler;
            }
            
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

        public async UniTask UpdateLiveOps(List<Transform> placements = null, bool debug = false)
        {
            var actions = _liveOps
                .Select(liveOps => (LiveOps: liveOps, Action: liveOps.CheckUpdateAction(debug)))
                .ToList();

            if (placements != null)
            {
                var placementIndex = 0;
                foreach (var liveOps in _liveOps)
                {
                    if (liveOps.CreateIcon(placements[placementIndex]) != null)
                        placementIndex++;
                }
            }

            await UniTask.WaitForSeconds(Settings.UpdateDelaySeconds, cancellationToken: default);
            await UniTask.WhenAll(actions.Select(x => x.LiveOps.UpdateVisual(_resourceEmitter, debug)));
            
            foreach (var updateAction in Settings.ProcessingOrder)
            {
                var group = actions
                    .Where(x => x.Action == updateAction && x.Action != UpdateAction.None)
                    .ToList();

                if (group.Count == 0)
                    continue;

                foreach (var tuple in group)
                    await tuple.LiveOps.UpdateState();
            }
        }
        
        public bool EvaluateConditions(LiveOpsData data)
        {
            foreach (var condition in data.Conditions)
            {
                var type = condition.GetType();
                if (!_handlers.TryGetValue(type, out var handler) || !handler.CanHandle(condition)) 
                    continue;
                if (!handler.Handle(condition))
                    return false;
            }
            return true;
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
