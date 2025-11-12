using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace TapEmpire.Services
{
    [Serializable]
    public class SystemService : Initializable, ISystemService
    {
        public Subject<bool> OnApplicationFocusChanged => _monoCallbackService.OnApplicationFocusChanged;

        [SerializeField]
        private MonoCallbacksService _monoCallbackServicePrefab = null;

        private MonoCallbacksService _monoCallbackService = null;

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
            }

            return UniTask.CompletedTask;
        }

        protected override void OnRelease() { }
    }
}