using System;
using System.Collections.Generic;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    [Serializable]
    public class AdsSettingsSerializable : RemoteSerializableBase<AdsSettings, AdsSettingsSerializable.AdsRemoteModel>
    {
        public override string TokenName => "AdsSettings";

        public class AdsRemoteModel : IRemoteModel<AdsSettings>
        {
            public bool? EnableMetica;
            public bool? EnableBanner;
            public bool? EnableMrec;
            public int? FromLevel;
            public int? BannerFromLevel;
            public int? RewardedFromLevel;
            public List<TimerData> TimerData;

            public void FromSettings(AdsSettings settings)
            {
                EnableMetica = settings.EnableMetica;
                EnableBanner = settings.EnableBanner;
                EnableMrec = settings.EnableMrec;
                FromLevel = settings.FromLevel;
                BannerFromLevel = settings.BannerFromLevel;
                RewardedFromLevel = settings.RewardedFromLevel;
                TimerData = settings.TimerData;
            }

            public void ToSettings(AdsSettings settings)
            {
                settings.EnableMetica = EnableMetica ?? settings.EnableMetica;
                settings.EnableBanner = EnableBanner ?? settings.EnableBanner;
                settings.EnableMrec = EnableMrec ?? settings.EnableMrec;
                settings.FromLevel = FromLevel ?? settings.FromLevel;
                settings.BannerFromLevel = BannerFromLevel ?? settings.BannerFromLevel;
                settings.RewardedFromLevel = RewardedFromLevel ?? settings.RewardedFromLevel;
                if (TimerData != null)
                {
                    settings.TimerData.Clear();
                    settings.TimerData.AddRange(TimerData);
                }
            }
        }
    }
}
