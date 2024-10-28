using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Services;
using TapEmpire.UI;
using TapEmpire.Utility;
using Zenject;
using TapEmpire.Settings;

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
        
        private SceneLoadingUIViewModel _sceneLoadingUIViewModel;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isInitialized = false;

        private ServicesContainer _servicesContainer;
        private ISceneManagementService _sceneManagementService;
        
        [Inject]
        private void Construct(ServicesContainer servicesContainer, ISceneManagementService sceneManagementService)
        {
            _servicesContainer = servicesContainer;
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
            await _servicesContainer.InitializeAsync(cancellationToken);
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
            if (_autoLoadSceneOnStart)
            {
                _sceneManagementService.LoadSceneAsync(_sceneName, _cancellationTokenSource.Token).Forget();
            }
        }
    }
}
