using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/SystemSettings", fileName = "SystemSettings")]
    public class SystemSettings : ScriptableObject
    {
        public float SessionInterval = 600.0f;
        public bool PlayOfflineForPayers = true;
    }
}