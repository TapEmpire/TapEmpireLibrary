using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Services.Analytics;
using Zenject;
using UnityEngine;
using Sirenix.Utilities;

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
        protected ISystemService _systemService;
        protected DiContainer _diContainer;
        protected ResourcesAnalyticsModule<T> _analyticsModule;

        protected Dictionary<T, ResourceRuntimeData<T>> _resources = new();

        protected CompositeDisposable _disposables = new();

        public ResourceRuntimeData<T> GetResourceData(T type) => _resources[type];

        [Inject]
        private void Construct(IProgressService progressService, DiContainer diContainer, ISystemService systemService)
        {
            _diContainer = diContainer;
            _progressService = progressService;
            _systemService = systemService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _settings.Resources.ForEach(resource => _resources.Add(resource.ResourceType, new ResourceRuntimeData<T>(resource, _progressService)));
            _systemService.OnApplicationFocusChanged.Subscribe(UpdateOnFocusChange).AddTo(_disposables);

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

        public void Set(T resource, int amount)
        {
            _resources[resource].Set(amount);
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

        public Sprite GetFlyingSprite(T type)
        {
            return GetSettings(type).FlyingSprite;
        }

        private ResourceSettings<T> GetSettings(T type)
        {
            return _settings.Resources.Find(setting => EqualityComparer<T>.Default.Equals(setting.ResourceType, type));
        }

        private void UpdateOnFocusChange(bool hasFocus)
        {
            _resources.ForEach(resource => resource.Value.RecheckAbsentTime());
        }
    }
}