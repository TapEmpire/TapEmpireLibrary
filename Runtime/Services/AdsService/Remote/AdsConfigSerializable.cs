using System;

namespace TapEmpire.Services
{
    [Serializable]
    public class AdsConfigSerializable : RemoteSerializableBase<AdsConfig, AdsConfigSerializable.AdsConfigRemoteModel>
    {
        public override string TokenName => "AdUnits";

        public class AdsConfigRemoteModel : IRemoteModel<AdsConfig>
        {
            public AdsDataRemoteModel AppLovin;
            public AdsDataRemoteModel Admob;

            public void FromSettings(AdsConfig settings)
            {
                AppLovin = new AdsDataRemoteModel();
                AppLovin.FromSettings(settings.AppLovin);
                Admob = new AdsDataRemoteModel();
                Admob.FromSettings(settings.Admob);
            }

            public void ToSettings(AdsConfig settings)
            {
                AppLovin?.ToSettings(settings.AppLovin);
                Admob?.ToSettings(settings.Admob);
            }
        }

        public class AdsDataRemoteModel : IRemoteModel<AdsData>
        {
            public string BannerId;
            public string MrecId;
            public string InterstitialId;
            public string RewardedId;

            public void FromSettings(AdsData settings)
            {
                BannerId = settings.BannerId;
                MrecId = settings.MrecId;
                InterstitialId = settings.InterstitialId;
                RewardedId = settings.RewardedId;
            }

            public void ToSettings(AdsData settings)
            {
                settings.BannerId = BannerId ?? settings.BannerId;
                settings.MrecId = MrecId ?? settings.MrecId;
                settings.InterstitialId = InterstitialId ?? settings.InterstitialId;
                settings.RewardedId = RewardedId ?? settings.RewardedId;
            }
        }
    }
}
