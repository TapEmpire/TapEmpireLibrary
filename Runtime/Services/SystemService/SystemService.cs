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

        private MonoCallbacksService _monoCallbackService = null;
        private DateTime _sessionTimeStamp = DateTime.UtcNow;
        private CompositeDisposable _disposables = new();

        [Inject]
        private void Construct()
        {
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (_monoCallbackService == null)
            {
                _monoCallbackService = Object.Instantiate(_monoCallbackServicePrefab);
                Object.DontDestroyOnLoad(_monoCallbackService.gameObject);

                OnApplicationFocusChanged.Subscribe(OnFocusChanged).AddTo(_disposables);
            }

            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            _disposables?.Dispose();
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
    }
}