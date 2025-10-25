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
using Sirenix.OdinInspector;

namespace TapEmpire.Game
{
    public class EntrySceneStarter : MonoBehaviour
    {
        [Header("Load scene")]
        [SerializeField][ShowIf("@_sceneSelector == null")]
        private SceneName _sceneName;

        [SerializeReference] ISceneSelector _sceneSelector;

        [SerializeField]
        private bool _autoLoadSceneOnStart = true;

        [SerializeField] private bool _shouldManualClose = false;

        [Header("Settings")]
        [SerializeField]
        private GameStartSettings _startSettings;

        [Header("Actions")]
        [SerializeReference]
        private IStartAction[] _startActions;
        
        private SceneLoadingUIViewModel _sceneLoadingUIViewModel;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isInitialized = false;

        private DiContainer _diContainer;
        private ServicesContainer _servicesContainer;
        private ISceneManagementService _sceneManagementService;
        
        [Inject]
        private void Construct(DiContainer diContainer, ServicesContainer servicesContainer, ISceneManagementService sceneManagementService)
        {
            _diContainer = diContainer;
            _servicesContainer = servicesContainer;
            _sceneManagementService = sceneManagementService;

            if (_sceneSelector != null)
            {
                diContainer.Inject(_sceneSelector);
            }
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

            _startActions.ForEach(action => {
                _diContainer.Inject(action);
                action.Apply();
            });

            _isInitialized = true;
        }

        private void Start()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = _startSettings.FrameRate;
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
                var sceneName = _sceneSelector != null ? _sceneSelector.GetNextScene() : _sceneName;
                _sceneManagementService.LoadSceneAsync(sceneName, _cancellationTokenSource.Token, _shouldManualClose).Forget();
            }
        }
    }
}
