using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Settings;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace TapEmpire.Services
{
    [Serializable]
    public class SystemService : Initializable, ISystemService, ITickable
    {
        public Subject<bool> OnApplicationFocusChanged => _monoCallbackService.OnApplicationFocusChanged;
        public Subject<Unit> OnSessionStarted { get; private set; } = new Subject<Unit>();
        public Observable<Unit> OnTick => _secondTick;

        [field: SerializeField] public SystemSettings SystemSettings { get; private set; }
        [field: SerializeField] public GameStartSettings StaticSettings { get; private set; }
        [SerializeField] private MonoCallbacksService _monoCallbackServicePrefab = null;

        public bool CanPlayOffline => CanPlayOfflineInternal();

        private IProgressService _progressService;
        private MonoCallbacksService _monoCallbackService = null;
        private DateTime _sessionTimeStamp = DateTime.UtcNow;
        private CompositeDisposable _disposables = new();
        private readonly Subject<Unit> _secondTick = new();
        private float _elapsed;

        [Inject]
        private void Construct(IProgressService progressService)
        {
            _progressService = progressService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _disposables = new();

            if (_monoCallbackService == null)
            {
                _monoCallbackService = Object.Instantiate(_monoCallbackServicePrefab);
                Object.DontDestroyOnLoad(_monoCallbackService.gameObject);

                // _monoCallbackService.OnApplicationFocusChanged.Subscribe(OnFocusChanged).AddTo(_disposables);
                _monoCallbackService.OnApplicationFocusChangedAction += OnFocusChanged;
            }

            SystemSettings.OnDataChanged.Subscribe(OnDataChanged).AddTo(_disposables);

            return UniTask.CompletedTask;
        }

        public void Tick()
        {
            _elapsed += Time.deltaTime;

            while (_elapsed >= 1f)
            {
                _elapsed -= 1f;
                _secondTick.OnNext(Unit.Default);
            }
        }

        protected override void OnRelease()
        {
            _monoCallbackService.OnApplicationFocusChangedAction -= OnFocusChanged;

            _disposables.Dispose();
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                var elapsed = (DateTime.UtcNow - _sessionTimeStamp).TotalSeconds;
                if (elapsed > SystemSettings.SessionInterval)
                {
                    OnSessionStarted.OnNext(Unit.Default);
                }
            }

            _sessionTimeStamp = DateTime.UtcNow;
        }

        private bool CanPlayOfflineInternal()
        {
            return _progressService.GetPlayOffline(SystemSettings.PlayOfflineForPayers) && _progressService.GetIsPayer() ||
                _progressService.GetPlayOfflineNoAds(SystemSettings.PlayOfflineNoAds) && _progressService.GetAdsDisabled() ||
                StaticSettings.IgnoreConnection;
        }

        private void OnDataChanged(SystemSettings settings)
        {
            _progressService.SetPlayOffline(settings.PlayOfflineForPayers);
            _progressService.SetPlayOfflineNoAds(settings.PlayOfflineNoAds);
        }
    }
}