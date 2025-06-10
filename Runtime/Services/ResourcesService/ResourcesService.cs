using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services.Analytics;
using Zenject;
using UnityEngine;
using Sirenix.Utilities;
using SlimeAway.CoreSystems;
using SlimeAway.Services;

namespace TapEmpire.Services
{
    public class ResourcesService<T> : Initializable, IResourcesService<T>
    {
        [SerializeField] private ResourcesSettings<T> _settings = null;

        private Subject<(T, int, string)> _onResourceAdded = new();
        private Subject<(T, int, string)> _onResourceUsed = new();
        private Subject<T> _onVirtualAdded = new();

        public Observable<(T, int, string)> OnResourceAdded => _onResourceAdded;
        public Observable<(T, int, string)> OnResourceUsed => _onResourceUsed;
        public Observable<T> OnVirtualAdded => _onVirtualAdded;

        protected IProgressService _progressService;
        protected DiContainer _diContainer;
        protected ResourcesAnalyticsModule<T> _analyticsModule;
        protected GameplaySettings _gameplaySettings;

        protected Dictionary<T, ResourceRuntimeData<T>> _resources = new();

        public ResourceRuntimeData<T> GetResourceData(T type) => _resources[type];

        [Inject]
        private void Construct(IProgressService progressService, DiContainer diContainer, IGameGenericService gameGenericService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _gameplaySettings = gameGenericService.GameplaySettings;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _settings.Resources.ForEach(resource => _resources.Add(resource.ResourceType, new ResourceRuntimeData<T>(resource, _progressService)));

            _analyticsModule = new ResourcesAnalyticsModule<T>(_diContainer);

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _analyticsModule?.OnRelease();
            _resources.ForEach(pair => pair.Value.Dispose());
            _resources.Clear();
            base.OnRelease();
        }

        public void Add(T resource, int amount, string reason = "")
        {
            var value = _resources[resource].Add(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceAdded.OnNext((resource, value, reason));
            }
        }

        public void Subtract(T resource, int amount, string reason = "")
        {
            var value = _resources[resource].Subtract(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceUsed.OnNext((resource, value, reason));
            }
        }

        public bool HasAmount(T resource, int amount)
        {
            return _resources[resource].HasAmount(amount);
        }

        public void AddVirtual(T resource, int amount, string reason)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceAdded.OnNext((resource, _resources[resource].Amount.Value + amount, reason));
            }
            _onVirtualAdded.OnNext(resource);
        }
    }
}