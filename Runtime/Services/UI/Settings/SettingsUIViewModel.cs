using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services;
using TEL.Services;
using Zenject;

namespace TapEmpire.UI
{
    public class SettingsUIViewModel : IUIViewModel, IInjectable
    {
        public readonly Action OnClose;

        private readonly SettingsToggleCode[] _activeToggles;

        private IAudioService _audioService;
        private IHapticService _hapticService;
        
        public SettingsUIViewModel(Action onClose, params SettingsToggleCode[] activeToggle)
        {
            OnClose = onClose;
            _activeToggles = activeToggle;
        }

        [Inject]
        private void Construct(IAudioService audioService, IHapticService hapticService)
        {
            _audioService = audioService;
            _hapticService = hapticService;
        }

        public bool CheckToggleActive(SettingsToggleCode code) => _activeToggles.Contains(code);

        public bool TryGetToggleData(SettingsToggleCode code, out Action<bool> callback, out bool startState)
        {
            callback = null;
            startState = false;
            if (!_activeToggles.Contains(code))
            {
                return false;
            }
            switch (code)
            {
                case SettingsToggleCode.Music:
                    callback = mode => _audioService.ChangeMusicMode(mode, true);
                    startState = _audioService.MusicMode;
                    return true;
                case SettingsToggleCode.Sound:
                    callback = mode => _audioService.ChangeSoundsMode(mode, true);
                    startState = _audioService.SoundsMode;
                    return true;
                case SettingsToggleCode.Vibration:
                    callback = mode =>
                    {
                        _hapticService.SetHapticsActive(mode, false);
                        if (mode)
                        {
                            _hapticService.PlayDefaultVibration();
                        }
                    };
                    startState = _hapticService.IsHapticsActive;
                    return true;
                default:
                    return false;
                        
            }
        }
    }
}