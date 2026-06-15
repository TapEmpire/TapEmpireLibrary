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
        [SerializeField] protected ResourcesSettings<T> _settings = null;

        private Subject<(T, int, string)> _onResourceAdded = new();
        private Subject<(T, int, string)> _onResourceUsed = new();
        private Subject<T> _onVirtualAdded = new();
        private Subject<(T resourceType, int amountAdd, ResourceAcquireType type, string source)> _onResourceAddedDetailed = new();
        private Subject<(T resourceType, int amountUsed, ResourceAcquireType type, string source)> _onResourceUsedDetailed = new();

        public Observable<(T, int, string)> OnResourceAdded => _onResourceAdded;
        public Observable<(T, int, string)> OnResourceUsed => _onResourceUsed;
        public Observable<T> OnVirtualAdded => _onVirtualAdded;
        public Observable<(T, int, ResourceAcquireType, string)> OnResourceAddedDetailed => _onResourceAddedDetailed;
        public Observable<(T, int, ResourceAcquireType, string)> OnResourceUsedDetailed => _onResourceUsedDetailed;

        protected IProgressService _progressService;
        protected ISystemService _systemService;
        protected DiContainer _diContainer;
        protected ResourcesAnalyticsModule<T> _analyticsModule;
#if TEL_CLOUD_SAVE
        protected CloudSaveResourceModule<T> _cloudSaveModule;
#endif

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
#if TEL_CLOUD_SAVE
            _cloudSaveModule = new CloudSaveResourceModule<T>(_diContainer, this);
#endif

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnRelease()
        {
            _analyticsModule?.OnRelease();
#if TEL_CLOUD_SAVE
            _cloudSaveModule?.OnRelease();
#endif
            _resources.ForEach(pair => pair.Value.Dispose());
            _resources.Clear();
            base.OnRelease();
        }

        public virtual void Add(T resource, int amount, string reason = "", ResourceAcquireType acquireType = ResourceAcquireType.Free)
        {
            var value = _resources[resource].Add(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceAdded.OnNext((resource, value, reason));
                _onResourceAddedDetailed.OnNext((resource, amount, acquireType, reason));
            }
        }

        public void Subtract(T resource, int amount, string reason = "", ResourceAcquireType acquireType = ResourceAcquireType.Free)
        {
            var value = _resources[resource].Subtract(amount);

            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceUsed.OnNext((resource, value, reason));
                _onResourceUsedDetailed.OnNext((resource, amount, acquireType, reason));
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

        public void Refresh()
        {
            _resources.ForEach(pair => pair.Value.RefreshFromProgress());
        }

        public virtual void AddVirtual(T resource, int amount, string reason, ResourceAcquireType acquireType = ResourceAcquireType.Free)
        {
            if (!string.IsNullOrEmpty(reason))
            {
                _onResourceAdded.OnNext((resource, _resources[resource].Amount.Value + amount, reason));
                _onResourceAddedDetailed.OnNext((resource, amount, acquireType, reason));
            }
            _onVirtualAdded.OnNext(resource);
        }

        public virtual Sprite GetFlyingSprite(T type)
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
    
    public enum ResourceAcquireType
    {
        Free,
        Rewarded,
        IAP
    }
}