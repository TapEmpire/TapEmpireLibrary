using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class IapSettingsSo<T> : ScriptableObject where T : IapSettings
    {
        [field: SerializeField] private List<T> _iaps = new();
        public List<T> Iaps => _iaps;
    }
}