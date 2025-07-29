using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using TapEmpire.Utility;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace TapEmpire.Services
{
    [Serializable]
    public class AudioService : Initializable, IAudioService
    {
        private const string MusicVolumeKey = "MusicVolume";
        private const string SoundsVolumeKey = "SoundsVolume";
        private const float AudioMinVolume = 0.0000001f;
        private const float AudioMaxVolume = 2.0f;

        [SerializeField]
        private AudioMixer _audioMixer;

        [SerializeReference]
        private IAudioBank _audioBank;
        
        [SerializeReference]
        private IAudioBank _customAudioBank;

        [Header("Sound")]
        [SerializeField]
        private AudioSource _soundSourcePrefab;

        [SerializeField]
        private AudioSource _soundAudio3DPrefab;

        [Header("Music")]
        [SerializeField]
        private AudioSource _musicAudioSourcePrefab;

        [NonSerialized]
        private AudioSource _backgroundMusicSource;
        [NonSerialized]
        private ComponentPool<AudioSource> _soundsSourcesPool;
        [NonSerialized]
        private ComponentPool<AudioSource> _soundsSources3DPool;

        private Dictionary<string, AudioSource> _audioSources;

        public float MusicVolume { get; private set; }
        public float SoundsVolume { get; private set; }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            _audioSources = new Dictionary<string, AudioSource>();

            _soundsSourcesPool?.Clear();
            _soundsSourcesPool = new ComponentPool<AudioSource>(_soundSourcePrefab, CreateSoundsPoolParent().transform);
            _soundsSources3DPool?.Clear();
            _soundsSources3DPool = new ComponentPool<AudioSource>(_soundAudio3DPrefab);

            return UniTask.CompletedTask;
        }

        private GameObject CreateSoundsPoolParent()
        {
            var root = new GameObject("SoundsPoolRoot");
            Object.DontDestroyOnLoad(root);

            return root;
        }

        protected override void OnRelease()
        {
            StopAllSounds();
            _soundsSourcesPool?.Clear();
            _soundsSources3DPool?.Clear();
        }

        public void InitializeMixer()
        {
            SetSoundsVolume(PlayerPrefsUtility.GetSoundSettings(1));
            SetMusicVolume(PlayerPrefsUtility.GetMusicSettings(1));
        }

        public void WarmUpSources(bool warmUpSoundsPool, bool warmUp3DSoundsPool, bool warmUpMusicInstance)
        {
            if (warmUpMusicInstance && _backgroundMusicSource == null)
            {
                _backgroundMusicSource = Object.Instantiate(_musicAudioSourcePrefab);
                Object.DontDestroyOnLoad(_backgroundMusicSource.gameObject);
            }
            if (warmUpSoundsPool)
            {
                var soundInstance = _soundsSourcesPool.Get();
                soundInstance.volume = 0;
                _soundsSourcesPool.Release(soundInstance);
            }
            if (warmUp3DSoundsPool)
            {
                var sound3DInstance = _soundsSources3DPool.Get();
                sound3DInstance.volume = 0;
                _soundsSources3DPool.Release(sound3DInstance);
            }
        }

        public AudioData GetAudioData<TAudioId>(TAudioId audioId) where TAudioId : Enum
        {
            return _audioBank.GetAudioData(audioId);
        }

        private void SetMusicVolume(float volume01)
        {
            volume01 = Mathf.Clamp(volume01, AudioMinVolume, AudioMaxVolume);
            MusicVolume = volume01;
            PlayerPrefsUtility.SetMusicSettings(volume01);
            _audioMixer.SetFloat(MusicVolumeKey, Mathf.Log10(volume01) * 20);
        }

        private void SetSoundsVolume(float volume01)
        {
            volume01 = Mathf.Clamp(volume01, AudioMinVolume, AudioMaxVolume);
            SoundsVolume = volume01;
            PlayerPrefsUtility.SetSoundSettings(volume01);
            _audioMixer.SetFloat(SoundsVolumeKey, Mathf.Log10(volume01) * 20);
        }

        public void PlaySoundOneShotAtPoint<TAudioId>(TAudioId audioId, Vector3 position, string uniqueId = "") where TAudioId : Enum
        {
            PlaySound(_soundsSources3DPool, audioId.ToString(), GetAudioData(audioId), uniqueId, position);
        }

        public void PlaySoundOneShot(string audioId, string uniqueId = "")
        {
            AudioData audioData = null;
            if (_customAudioBank.HasAudioData(audioId))
            {
                audioData = _customAudioBank.GetAudioData(audioId);
            }
            else if (_audioBank.HasAudioData(audioId))
            {
                audioData = _audioBank.GetAudioData(audioId);
            }
            else
            {
                Debug.LogWarning($"No audioId in banks {audioId}");
                return;
            }
            
            PlaySound(_soundsSourcesPool, audioId, audioData, uniqueId);
        }

        public void PlaySoundOneShot<TAudioId>(TAudioId audioId, string uniqueId = "") where TAudioId : Enum
        {
            PlaySound(_soundsSourcesPool, audioId.ToString(), GetAudioData(audioId), uniqueId);
        }

        public void SetCustomAudioBank(IAudioBank audioBank)
        {
            _customAudioBank = audioBank;
        }

        private void PlaySound(ComponentPool<AudioSource> pool, string audioId, AudioData audioData, string uniqueId, Vector3? position = null)
        {
            AudioSource instance = pool.GetSafe();

            if (instance == null)
            {
                Debug.LogWarning($"[AUDIO] No available AudioSource in pool for sound: {audioId}");
                return;
            }

            if (position.HasValue)
            {
                instance.transform.position = position.Value;
            }

            if (audioData == null)
            {
                Debug.LogWarning($"[AUDIO] AudioData for {audioId} not found.");
                return;
            }

            instance.SetupData(audioData);
            instance.Play();

            var instanceKey = audioId + uniqueId;
            _audioSources.TryAdd(instanceKey, instance);
            
            var length = audioData.Clip.length;
            ScheduleRelease(pool, instanceKey, instance, length);
        }

        public void PauseLoopSounds()
        {
            _audioSources.ForEach(x =>
            {
                if (x.Value.loop)
                {
                    x.Value.Pause();
                }
            });
        }
        
        public void ResumeLoopSounds()
        {
            _audioSources.ForEach(x =>
            {
                if (x.Value.loop)
                {
                    x.Value.Play();
                }
            });
        }

        private void ScheduleRelease(ComponentPool<AudioSource> pool, string instanceKey, AudioSource instance, float delay)
        {
            UniTaskUtility.ExecuteAfterSeconds(delay + 1.0f, () =>
            {
                if (instance != null && !instance.isPlaying)
                {
                    _audioSources.Remove(instanceKey);
                    instance.Stop();
                    instance.clip = null;
                    instance.loop = false;
                    pool.Release(instance);
                }
            }, Application.exitCancellationToken).Forget();
        }

        public void StartPlayMusic<TAudioId>(TAudioId audioId, float fadeInDuration = 0.5f) where TAudioId : Enum
        {
            var audioData = GetAudioData(audioId);
            _backgroundMusicSource.SetupDataWithVolumeFadeIn(audioData, fadeInDuration);
            _backgroundMusicSource.Play();
        }

        public void PlaySoundLoop(string audioId, string uniqueId = "")
        {
            PlaySoundLoop(_soundsSourcesPool, audioId, _customAudioBank.GetAudioData(audioId), uniqueId);
        }

        public void PlaySoundLoop<TAudioId>(TAudioId audioId, string uniqueId = "") where TAudioId : Enum
        {
            PlaySoundLoop(_soundsSourcesPool, audioId.ToString(), GetAudioData(audioId), uniqueId);
        }

        private void PlaySoundLoop(ComponentPool<AudioSource> pool, string audioId, AudioData audioData, string uniqueId, Vector3? position = null)
        {
            var instance = pool.GetSafe();

            if (instance == null)
            {
                Debug.LogWarning($"No available AudioSource in pool for sound: {audioId}");
                return;
            }
            
            if (audioData == null)
            {
                Debug.LogWarning($"[AUDIO] AudioData for {audioId} not found.");
                return;
            }

            if (position.HasValue)
            {
                instance.transform.position = position.Value;
            }
            
            instance.loop = true;

            instance.SetupData(audioData);
            instance.Play();

            _audioSources.TryAdd(audioId + uniqueId, instance);
        }

        public void StopSound(string audioId, string uniqueId = "")
        {
            StopSound(_soundsSourcesPool, audioId, uniqueId);
        }

        public void StopSound<AudioId>(AudioId audioId, string uniqueId = "") where AudioId : Enum
        {
            StopSound(_soundsSourcesPool, audioId.ToString(), uniqueId);
        }

        private void StopSound(ComponentPool<AudioSource> pool, string audioId, string uniqueId, Vector3? position = null)
        {
            if (!_audioSources.TryGetValue(audioId + uniqueId, out AudioSource source))
            {
                return;
            }

            if (source != null)
            {
                source.Stop();
                source.loop = false;
                source.clip = null;
                pool.Release(source);
            }

            if (_audioSources != null && _audioSources.Count > 0)
            {
                _audioSources.Remove(audioId + uniqueId);
            }
        }

        public void StopAllSounds()
        {
            if (_audioSources != null && _audioSources?.Count > 0)
            {
                foreach (var source in _audioSources)
                {
                    if (source.Value != null)
                    {
                        source.Value.loop = false;
                        source.Value.Stop();
                        source.Value.clip = null;
                    }
                }

                _audioSources?.Clear();
            }
        }

        public bool MusicMode => MusicVolume > AudioMinVolume;

        // TODO fade
        public void ChangeMusicMode(bool mode, bool withFade)
        {
            SetMusicVolume(mode ? 1 : 0);
        }

        public bool SoundsMode => SoundsVolume > AudioMinVolume;

        // TODO fade
        public void ChangeSoundsMode(bool mode, bool withFade)
        {
            SetSoundsVolume(mode ? 1 : 0);
        }
    }
}
