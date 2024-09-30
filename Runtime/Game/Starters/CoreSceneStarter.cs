using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Services;
using TapEmpire.CoreSystems;
using TapEmpire.UI;
using Zenject;
using TEL.Services;
using TapEmpire.Settings;
using Game.Services;
using TapEmpire.Utility;

namespace TapEmpire.Game
{
    public class CoreSceneStarter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private GameStartSettings _startSettings;

        [Header("Context")]
        [SerializeField]
        private SceneContext _coreSceneContext;

        // [Header("UI")]
        // [SerializeField]
        // private CoreDebugUIView _coreDebugUIView;

        // [SerializeField]
        // private CoreHudUIView _coreHudUIView;
        
        private IService[] _services;
        private ICoreSystem[] _systems;
        private DiContainer _diContainer;
        private ITicksContainer _ticksContainer;
        
        private ISceneContextsService _sceneContextsService;
        // private ILevelExecutionCoreSystem _levelExecutionCoreSystem;
        private IUIService _uiService;
        private IProgressService _progressService;
        private IAudioService _audioService;
        private IHapticService _hapticService;
        
        [Inject]
        private void Construct(IService[] services, ICoreSystem[] systems, ISceneContextsService sceneContextsService, DiContainer diContainer,
            IUIService uiService, IProgressService progressService, IAudioService audioService, IHapticService hapticService,
            ITicksContainer ticksContainer)
        {
            _services = services;
            _systems = systems;
            _sceneContextsService = sceneContextsService;
            _diContainer = diContainer;

            // _levelExecutionCoreSystem = levelExecutionCoreSystem;
            _progressService = progressService;

            _uiService = uiService;
            _audioService = audioService;
            _hapticService = hapticService;
        }

        private void Awake()
        {
            InstallSceneAsync(Application.exitCancellationToken).Forget();
        }

        private async UniTask InstallSceneAsync(CancellationToken cancellationToken)
        {
            await InitializableUtility.InitializeAsync(_services, _diContainer, cancellationToken);
            _ticksContainer.InitializeTicks(_services);
            await InitializableUtility.InitializeAsync(_systems, _diContainer, cancellationToken);
            _ticksContainer.InitializeTicks(_systems);
            _sceneContextsService.AddInstalledSceneContext("Core", _coreSceneContext);

            _audioService.InitializeMixer();
            // _audioService.WarmUpSources(true, false, false);
            _hapticService.SetHapticsActive(true, false);
            
#if UNITY_EDITOR
            // var levelsOnScene = FindObjectsOfType<LevelView>();
            // foreach (var levelOnScene in levelsOnScene)
            // {
            //     Destroy(levelOnScene.gameObject);
            // }
#endif

            /*if (_startSettings.Debug)
            {
                _uiService.OpenViewAsync(_coreDebugUIView, new CoreDebugUIViewModel(), CancellationToken.None).Forget();
            }
            _uiService.OpenViewAsync(_coreHudUIView, new CoreHudUIViewModel(), CancellationToken.None).Forget();*/
            
            // var level = GetStartLevelIndex();
            // _levelExecutionCoreSystem.StartLevel(level);
        }

        private int GetStartLevelIndex()
        {
            #if UNITY_EDITOR
            if (_startSettings.Debug && !_startSettings.EditorStartFromPrefLevel)
            {
                return _startSettings.EditorDebugStartLevelIndex;
            }
            #endif
            return _progressService.GetLevelProgress();
        }
        
        private void OnDestroy()
        {
            _ticksContainer.ReleaseTicks(_systems);
            foreach (var system in _systems)
            {
                system.Release();
            }
                
            _ticksContainer.ReleaseTicks(_services);
            foreach (var service in _services)
            {
                service.Release();
            }
        }
    }
}