using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RagDoll.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TapEmpire.UI;
using UnityEngine.ResourceManagement.ResourceProviders;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class SceneManagementService : Initializable, ISceneManagementService
    {
        [SerializeField]
        private SceneLoadingUIView _sceneLoadingUIPrefab;
        
        [SerializeField]
        private float _animationDurationPerFullProgress = 2f;
        
        private IUIService _uiService;

        private SceneLoadingUIViewModel _sceneLoadingUIViewModel;

        private readonly float _minDisplayTime = 1.5f; // Minimum time to show the progress bar
        
        [Inject]
        private void Construct(IUIService uiService)
        {
            _uiService = uiService;
        }

        public async UniTask LoadSceneAsync(SceneName sceneName, CancellationToken cancellationToken)
        {
            _sceneLoadingUIViewModel = new SceneLoadingUIViewModel();
            await _uiService.OpenViewAsync(_sceneLoadingUIPrefab, _sceneLoadingUIViewModel, cancellationToken);
            
            var currentProgress = 0f;
            var startTime = Time.time;

            AsyncOperationHandle<SceneInstance> sceneHandle = Addressables.LoadSceneAsync(sceneName.ToString(), activateOnLoad: false);
            await sceneHandle.ToUniTask(
                Progress.Create<float>(progress =>
                {
                    var progressChange = progress - currentProgress;
                    var duration = progressChange * _animationDurationPerFullProgress;
                    currentProgress = progress;
                    _sceneLoadingUIViewModel.SetProgressCallback(progress, duration);
                }), cancellationToken: cancellationToken);
            
            var elapsedTime = Time.time - startTime;
            if (currentProgress < 1f && elapsedTime < _minDisplayTime)
            {
                var progressChange = 1 - currentProgress;
                var duration = progressChange * _animationDurationPerFullProgress / 2;
                _sceneLoadingUIViewModel.SetProgressCallback(1, duration);
                await UniTask.WaitForSeconds(duration, cancellationToken: cancellationToken);
            }

            sceneHandle.Result.ActivateAsync().completed += _ =>
            {
                _uiService.TryCloseViewAsync<SceneLoadingUIViewModel>(cancellationToken).Forget();
            };
        }
    }
}
