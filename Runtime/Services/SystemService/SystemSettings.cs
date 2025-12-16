using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/SystemSettings", fileName = "SystemSettings")]
    public class SystemSettings : ScriptableObject
    {
        public readonly Subject<SystemSettings> OnDataChanged = new();

        public float SessionInterval = 600.0f;
        public bool PlayOfflineForPayers = true;

        public void BroadcastUpdate() => OnDataChanged.OnNext(this);
    }
}