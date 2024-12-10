using System;
using UnityEngine;

namespace TapEmpire.Services
{
    public interface IAudioService : IService
    {
        // почему-то миксер не воспринимает попытки задать громкость раньше Start (т.е. ни в инсталлере, ни в Awake)
        void InitializeMixer();
        void WarmUpSources(bool warmUpSoundsPool, bool warmUp3DSoundsPool, bool warmUpMusicInstance);
        
        AudioData GetAudioData<AudioId>(AudioId audioId) where AudioId : Enum;

        void PlaySoundOneShotAtPoint<AudioId>(AudioId audioId, Vector3 position, string uniqueId = "") where AudioId : Enum;
        
        void PlaySoundOneShot<AudioId>(AudioId audioId, string uniqueId = "") where AudioId : Enum;

        void StartPlayMusic<AudioId>(AudioId audioId, float fadeInDuration = 0.5f) where AudioId : Enum;
        void PlaySoundLoop<AudioId>(AudioId audioId, string uniqueId = "") where AudioId : Enum;
        void StopSound<AudioId>(AudioId audioId, string uniqueId = "") where AudioId : Enum;
        void StopAllSounds();

        bool MusicMode { get; }
        void ChangeMusicMode(bool mode, bool withFade);
        
        bool SoundsMode { get; }

        void ChangeSoundsMode(bool mode, bool withFade);
    }
}
