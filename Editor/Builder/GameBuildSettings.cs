using System;
using UnityEngine;

namespace TapEmpire.Build
{
    [Serializable]
    public class AdsData
    {
        public string AppKey; // might be appId or sdkId
        public string BannerId;
        public string MrecId;
        public string InterstitialId;
        public string RewardedId;
    }

    [Serializable]
    public class AdjustData
    {
        public string AppToken;
        public string PurchaseToken;
        public string EventToken;
    }

    [Serializable]
    public class PlatformData
    {
        public AdjustData Adjust;
        public AdsData ApplovinAds;
        public AdsData GoogleAds;
    }

    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameBuildSettings", fileName = "GameBuildSettings")]
    public class GameBuildSettings : ScriptableObject
    {
        public PlatformData Android;
        public PlatformData Ios;

        [SerializeReference][Space(5)]
        public IBuildAction[] BuildActions;
    }
}