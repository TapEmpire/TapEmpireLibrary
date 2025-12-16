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
    public class SystemService : Initializable, ISystemService
    {
        public Subject<bool> OnApplicationFocusChanged => _monoCallbackService.OnApplicationFocusChanged;
        public Subject<Unit> OnSessionStarted { get; private set; } = new Subject<Unit>();

        [SerializeField] private SystemSettings _settings;
        [field: SerializeField] public GameStartSettings StaticSettings { get; private set; }
        [SerializeField] private MonoCallbacksService _monoCallbackServicePrefab = null;

        public bool CanPlayOffline => CanPlayOfflineInternal();

        private IProgressService _progressService;
        private MonoCallbacksService _monoCallbackService = null;
        private DateTime _sessionTimeStamp = DateTime.UtcNow;
        private CompositeDisposable _disposables = new();

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

            _settings.OnDataChanged.Subscribe(OnDataChanged).AddTo(_disposables);

            return UniTask.CompletedTask;
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
                if (elapsed > _settings.SessionInterval)
                {
                    OnSessionStarted.OnNext(Unit.Default);
                }
            }

            _sessionTimeStamp = DateTime.UtcNow;
        }

        private bool CanPlayOfflineInternal()
        {
            return _progressService.GetPlayOffline(_settings.PlayOfflineForPayers) && _progressService.GetIsPayer() || StaticSettings.IgnoreConnection;
        }

        private void OnDataChanged(SystemSettings settings)
        {
            _progressService.SetPlayOffline(settings.PlayOfflineForPayers);
        }
    }
}