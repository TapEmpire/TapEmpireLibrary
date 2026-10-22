using System;
using TapEmpire.Game;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class MusicStartAction : IStartAction
    {
        [Inject]
        private IAudioService _audioService;

        public void Apply()
        {
            _audioService.InitializeMixer();
            _audioService.WarmUpSources(false, false, true);
            _audioService.StartMusicTracks();
        }
    }
}
