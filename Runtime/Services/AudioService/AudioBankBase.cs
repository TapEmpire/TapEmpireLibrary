using System;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public abstract class IAudioBank : ScriptableObject
    {
        public abstract AudioData GetAudioData<AudioId>(AudioId audioId) where AudioId : Enum;
    }

    public class AudioBankBase<AudioId> : IAudioBank where AudioId : Enum
    {
        public SerializableDictionary<AudioId, AudioData> AudioDataDictionary;

        public override AudioData GetAudioData<AudioId1>(AudioId1 audioId)
        {
            // Unsafe conversion.
            var value = audioId.ToInt();
            return AudioDataDictionary[EnumUtility.Parse<AudioId>(value)];
        }
    }
}
