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

        public bool Debug;
        public int FrameRate = 60;

        public float SessionInterval = 600.0f;
        public bool PlayOfflineForPayers = true;
        public bool PlayOfflineNoAds = true;
        public bool IgnoreConnection = false;

        public bool IsPushServiceEnabled;

        public void BroadcastUpdate() => OnDataChanged.OnNext(this);
    }
}