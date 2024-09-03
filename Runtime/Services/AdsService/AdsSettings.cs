
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsSettings", fileName = "AdsSettings")]
    public class AdsSettings : ScriptableObject
    {
        [Header ("On-Off")]
        public bool EnableAppOpen = true;

        [Space(5)]
        public List<int> InterstitialAfterLevels = new();
    }
}