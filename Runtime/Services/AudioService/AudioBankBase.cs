using System;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public abstract class IAudioBank : ScriptableObject
    {
        public abstract AudioData GetAudioData<TAudioId>(TAudioId audioId) where TAudioId : Enum;

        public abstract AudioData GetAudioData(string audioId);
    }

    public class AudioBankBase<TAudioId> : IAudioBank where TAudioId : Enum
    {
        public SerializableDictionary<TAudioId, AudioData> AudioDataDictionary;

        public override AudioData GetAudioData<AudioId1>(AudioId1 audioId)
        {
            // Unsafe conversion.
            var value = audioId.ToInt();
            return AudioDataDictionary[EnumUtility.Parse<TAudioId>(value)];
        }

        public override AudioData GetAudioData(string audioId)
        {
            return AudioDataDictionary[EnumUtility.Parse<TAudioId>(audioId)];
        }
    }
}
