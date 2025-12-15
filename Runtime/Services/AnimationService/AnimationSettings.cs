using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AnimationSettings", fileName = "AnimationSettings")]
    public class AnimationSettings : ScriptableObject
    {
        [Header("UI animations")]
        public float ScatterRadius = 200.0f;
        public float ScatterRandomness = 50.0f;
    }
}