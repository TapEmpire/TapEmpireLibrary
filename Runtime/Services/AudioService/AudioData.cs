using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class AudioData
    {
        public AudioClip Clip;
        public float Pitch = 1f;
        public float Volume = 1f;

        public AudioData Clone()
        {
            return new AudioData
            {
                Clip = Clip,
                Pitch = Pitch,
                Volume = Volume
            };
        }
    }
}