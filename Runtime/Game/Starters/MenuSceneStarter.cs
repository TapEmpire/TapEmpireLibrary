using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services;
using TapEmpire.UI;
using TEL.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Game
{
    public class MenuSceneStarter : SceneStarter
    {
        [SerializeField] private Slider _loadingSlider;
        [SerializeField] private TMP_Text _prevLevelText;
        [SerializeField] private TMP_Text _currentLevelText;
        [SerializeField] private Button _startLevelButton;
        [SerializeField] private Button _settingsButton;
        
        private IProgressService _progressService;
        private ISceneManagementService _sceneManagementService;
        private IUIService _uiService;
        
        private IAudioService _audioService;
        private IHapticService _hapticService;

        [Inject]
        private void Construct(
            IProgressService progressService,
            ISceneManagementService sceneManagementService,
            IUIService uiService, 
            IAudioService audioService, 
            IHapticService hapticService)
        {
            _progressService = progressService;
            _sceneManagementService = sceneManagementService;
            _uiService = uiService;
            _audioService = audioService;
            _hapticService = hapticService;
        }
        
        protected override void OnServicesInitialized()
        {
            if (!_progressService.TryGetIntProp(ProgressIntProp.CompletedLevelCount, out var levelValue)) 
                return;
            _prevLevelText.text = levelValue.ToString();
            _currentLevelText.text = (levelValue + 1).ToString();
            _startLevelButton.onClick.AddListener(StartLevel);
            ConfigureSettings();
        }
        
        private void ConfigureSettings()
        {
            // _settingsButton.onClick.AddListener(OpenSettings);
            // _settingsViewModel = new PausePopupViewModel
            // {
            //     AudioService = _audioService,
            //     HapticService = _hapticService,
            //     ShowRetry = false
            // };
            // _settingsViewModel.OnClose += () => _uiService.TryCloseViewAsync<PausePopupViewModel>(CancellationToken.None).Forget();
            // _settingsViewModel.OnRetry += () => { };
            
            // _audioService.InitializeMixer();
            // _audioService.StartPlayMusic(AudioID.BackgroundMusic);
        }

        private async void StartLevel()
        {
            // _uiService.TryCloseViewAsync<LifePanelViewModel>(CancellationToken.None).Forget();
            _uiService.CloseAllViewsExcept<SceneLoadingUIViewModel>(CancellationToken.None, false).Forget();
            await _sceneManagementService.LoadSceneAsync(SceneName.Core, CancellationToken.None);
        }
        
        private void OpenSettings()
        {
            // _uiService.OpenViewAsync(_settingsPopupView, _settingsViewModel, CancellationToken.None).Forget();
        }

        private void OnDestroy()
        {
            // _uiService.Release();
        }
    }
}