using System;
using UnityEngine;
using UnityEngine.Audio;

namespace TapEmpire.Services
{
    public interface IAudioService : IService
    {
        // почему-то миксер не воспринимает попытки задать громкость раньше Start (т.е. ни в инсталлере, ни в Awake)
        void InitializeMixer();
        void WarmUpSources(bool warmUpSoundsPool, bool warmUp3DSoundsPool, bool warmUpMusicInstance);
        
        AudioData GetAudioData<TAudioId>(TAudioId audioId) where TAudioId : Enum;
        AudioMixer GetMixer();
        string GetCurrentMixerSnapshot();

        void PlaySoundOneShotAtPoint<TAudioId>(TAudioId audioId, Vector3 position, string uniqueId = "") where TAudioId : Enum;

        void PlaySoundOneShot(string audioId, string uniqueId = "");
        void PlaySoundOneShot<TAudioId>(TAudioId audioId, string uniqueId = "") where TAudioId : Enum;

        void StartPlayMusic<TAudioId>(TAudioId audioId, float fadeInDuration = 0.5f) where TAudioId : Enum;
        void PlaySoundLoop(string audioId, string uniqueId = "");
        void PlaySoundLoop<TAudioId>(TAudioId audioId, string uniqueId = "") where TAudioId : Enum;
        void StopSound(string audioId, string uniqueId = "");
        void StopSound<TAudioId>(TAudioId audioId, string uniqueId = "") where TAudioId : Enum;
        void StopAllSounds();
        public void PauseLoopSounds();
        public void ResumeLoopSounds();
        void SetCustomAudioBank(IAudioBank audioBank);

        bool MusicMode { get; }
        void ChangeMusicMode(bool mode, bool withFade);
        
        bool SoundsMode { get; }

        void ChangeSoundsMode(bool mode, bool withFade);
        void SetMixerSnapshot(string snapshotId, float transitionTime);
    }
}
