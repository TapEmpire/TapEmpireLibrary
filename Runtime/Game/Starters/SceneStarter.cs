using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.Game
{
    public abstract class SceneStarter : MonoBehaviour
    {
        [Inject]
        protected DiContainer DiContainer;

        [Inject]
        private ServicesContainer _servicesContainer;
        
        private void Awake()
        {
            StartSceneAsync(Application.exitCancellationToken).Forget();
        }

        private async UniTask StartSceneAsync(CancellationToken cancellationToken)
        {
            await _servicesContainer.InitializeAsync(cancellationToken);
            OnServicesInitialized();
        }

        protected abstract void OnServicesInitialized();
    }
}