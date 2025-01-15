using DG.Tweening;
using UnityEngine;

namespace TapEmpire.Services
{
    public static class AudioSourceExtensions
    {
        public static void SetupData(this AudioSource source, AudioData data)
        {
            source.clip = data.Clip;
            source.volume = data.Volume;
            source.pitch = data.Pitch;
        }
        
        public static AudioData SetupDataWithVolumeFadeIn(this AudioSource source, AudioData data, float fadeInDuration = 0.5f)
        {
            source.volume = 0;
            source.clip = data.Clip;
            source.pitch = data.Pitch;
            source.DOFade(data.Volume, fadeInDuration).OnComplete(() =>
            {
                source.volume = data.Volume;
            });
            return data;
        }
    }
}