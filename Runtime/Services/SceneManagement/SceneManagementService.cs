using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        [SerializeField] private float _initialProgress = 0.3f;
        [SerializeField] private float _adsProgress = 0.8f;

        private IUIService _uiService;
        private IAdsService _adsService;

        private SceneLoadingUIViewModel _sceneLoadingUIViewModel;

        private readonly float _minDisplayTime = 1.5f; // Minimum time to show the progress bar
        private readonly float _initialProgressTime = 0.5f;
        private float _initialProgressDone = 0.0f;

        private UniTaskCompletionSource _completionSource = null;

        [Inject]
        private void Construct(IUIService uiService, IAdsService adsService)
        {
            _uiService = uiService;
            _adsService = adsService;
        }

        public async UniTask CreateLoadingScreen(CancellationToken cancellationToken)
        {
            await CreateLoadingScreenInternal(_initialProgress, _initialProgressTime, cancellationToken);
            WaitAdsService(cancellationToken).Forget();
        }

        private async UniTask WaitAdsService(CancellationToken cancellationToken)
        {
            if (_adsService.MaxWaitingTime > float.Epsilon)
            {
                await UniTask.WaitForSeconds(_initialProgressTime, cancellationToken: cancellationToken);
                _sceneLoadingUIViewModel?.SetProgressCallback(_adsProgress, _adsService.MaxWaitingTime);
                _initialProgressDone = _adsProgress;
            }
        }

        private async UniTask CreateLoadingScreenInternal(float initialProgress, float initialTime, CancellationToken cancellationToken)
        {
            _sceneLoadingUIViewModel = new SceneLoadingUIViewModel();
            await _uiService.OpenViewAsync(_sceneLoadingUIPrefab, _sceneLoadingUIViewModel, cancellationToken);

            if (_sceneLoadingUIViewModel == null)
            {
                
            }
            _sceneLoadingUIViewModel.SetProgressCallback(initialProgress, initialTime);
            _initialProgressDone = initialProgress;
        }

        public async UniTask CloseLoadingScreen(CancellationToken cancellationToken)
        {
            _sceneLoadingUIViewModel = null;
            await _uiService.TryCloseViewAsync<SceneLoadingUIViewModel>(cancellationToken);
        }

        public async UniTask LoadSceneAsync(SceneName sceneName, CancellationToken cancellationToken, bool manualLoadingClose = false)
        {
            var initialProgress = _initialProgressDone; // _sceneLoadingUIViewModel != null ? _initialProgress : 0.0f;

            if (_sceneLoadingUIViewModel == null)
            {
                await CreateLoadingScreenInternal(0.0f, 0.0f, cancellationToken);
            }

            var currentProgress = 0f;
            var startTime = Time.time;
            var koef = 1.0f - initialProgress;

            AsyncOperationHandle<SceneInstance> sceneHandle = Addressables.LoadSceneAsync(sceneName.ToString(), activateOnLoad: false);
            await sceneHandle.ToUniTask(
                Progress.Create<float>(progress =>
                {
                    var progressChange = progress - currentProgress;
                    var duration = progressChange * _animationDurationPerFullProgress;
                    currentProgress = progress;
                    _sceneLoadingUIViewModel?.SetProgressCallback(initialProgress + koef * progress, duration);
                }), cancellationToken: cancellationToken);

            var elapsedTime = Time.time - startTime;
            if (currentProgress < 1f && elapsedTime < _minDisplayTime)
            {
                var progressChange = 1 - currentProgress;
                var duration = progressChange * _animationDurationPerFullProgress / 2;
                _sceneLoadingUIViewModel?.SetProgressCallback(1, duration);
                await UniTask.WaitForSeconds(duration, cancellationToken: cancellationToken);
            }

            // await UniTask.WaitUntil(() => _adsService.ShouldWaitAppOpen == false);

            Action<AsyncOperation> onCompleted = manualLoadingClose ? (_) => { } :
                (_) =>
                {
                    _adsService.ShowAppOpen(() => {
                        _uiService.TryCloseViewAsync<SceneLoadingUIViewModel>(cancellationToken).Forget();
                        _sceneLoadingUIViewModel = null;
                    });
                };

            sceneHandle.Result.ActivateAsync().completed += onCompleted;
        }
    }
}
