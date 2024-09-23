using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using RagDoll.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TapEmpire.Services;
using TapEmpire;
using TapEmpire.UI;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Game
{
    public class EntrySceneStarter : MonoBehaviour
    {
        [Header("Load scene")]
        [SerializeField]
        private SceneName _sceneName;

        [SerializeField]
        private bool _autoLoadSceneOnStart = true;

        [Header("Settings")]
        [SerializeField]
        private GameStartSettings _startSettings;

        private IService[] _services;
        private DiContainer _diContainer;
        private ISceneManagementService _sceneManagementService;

        private SceneLoadingUIViewModel _sceneLoadingUIViewModel;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _isInitialized = false;

        [Inject]
        private void Construct(IService[] services, DiContainer diContainer, ISceneManagementService sceneManagementService)
        {
            _services = services;
            _diContainer = diContainer;
            _sceneManagementService = sceneManagementService;
        }

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource(); // CancellationTokenSource.CreateLinkedTokenSource(Application.exitCancellationToken);
            InstallServices(_cancellationTokenSource.Token).Forget();
        }

        private void OnDestroy()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async UniTask InstallServices(CancellationToken cancellationToken)
        {
            await InitializableUtility.InitializeAsync(_services, _diContainer, cancellationToken);
            _isInitialized = true;
        }

        private void Start()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Application.targetFrameRate = 30;
#endif
            Debug.unityLogger.filterLogType = (Debug.isDebugBuild || _startSettings.Debug) ? LogType.Log : LogType.Assert;
            InitializeEntryAsync().Forget();
        }

        private async UniTask InitializeEntryAsync()
        {
            if (_autoLoadSceneOnStart)
            {
                await _sceneManagementService.CreateLoadingScreen(_cancellationTokenSource.Token);
            }

            await UniTask.WaitUntil(() => _isInitialized, cancellationToken: _cancellationTokenSource.Token);
            await NetworkUtility.WaitNetworkAsync(Application.exitCancellationToken);

            if (_autoLoadSceneOnStart)
            {
                _sceneManagementService.LoadSceneAsync(_sceneName, _cancellationTokenSource.Token).Forget();
            }
        }
    }
}
