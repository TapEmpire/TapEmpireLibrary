using System;

namespace TapEmpire.Services
{
    [Serializable]
    public class AdsData
    {
        public string AppKey;
        public string BannerId;
        public string MrecId;
        public string InterstitialId;
        public string RewardedId;

        public void CopyFrom(AdsData other)
        {
            AppKey = other.AppKey;
            BannerId = other.BannerId;
            MrecId = other.MrecId;
            InterstitialId = other.InterstitialId;
            RewardedId = other.RewardedId;
        }
    }
}
