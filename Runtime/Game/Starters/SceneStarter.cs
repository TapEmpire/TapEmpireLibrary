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
        
        private void Start()
        {
            StartSceneAsync(Application.exitCancellationToken).Forget();
        }

        private async UniTask StartSceneAsync(CancellationToken cancellationToken)
        {
            Debug.Log("StartSceneAsync");
            await _servicesContainer.InitializeAsync(cancellationToken);
            Debug.Log("StartSceneAsync complete");
            OnServicesInitialized();
        }

        protected abstract void OnServicesInitialized();
    }
}