using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Settings
{
    public enum WinFlow
    {
        Next,
        Menu,
    }

    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameSettings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public bool AutoRestartLevel;

        [Header("Ads")]
        public bool AdsOnQuit = false;
        public bool AdsOnRestart = true;

        [Header("Win flow")]
        public WinFlow WinFlow = WinFlow.Next;
        public List<int> WinFlowExceptionLevels = new();
    }
}
