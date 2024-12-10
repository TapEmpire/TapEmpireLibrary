using System;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class AudioData
    {
        public AudioClip Clip;
        public float Volume = 1;
        public float Pitch = 1;
    }
}