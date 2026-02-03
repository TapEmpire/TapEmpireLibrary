using R3;
using UnityEngine;
using System.Collections.Generic;
using TapEmpire.Services;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;

namespace TapEmpire.Modules
{
    [CreateAssetMenu(menuName = "TapEmpire/Modules/SegmentationSettings", fileName = "SegmentationSettings")]
    public class SegmentationSettings : ScriptableObject
    {
        public AdsSettings AdsSettings;
        public List<CampaignSettings> Campaigns;

        public void UpdateData()
        {
            var campaignName = ProgressServiceExtensions.GetCampaignNameStatic();

            if (!string.IsNullOrEmpty(campaignName))
            {
                var settings = Campaigns.Find(campaign => campaignName.StartsWith(campaign.Name));
                if (settings != null)
                {
                    UpdateData(settings);
                }
            }
        }

        private void UpdateData(CampaignSettings settings)
        {
            AdsSettings.EnableBanners = settings.EnableBanners;
            AdsSettings.FromLevel = settings.FromLevel;
            AdsSettings.TimerData = settings.TimerData.ToList();
            AdsSettings.RewardedFromLevel = settings.RewardedFromLevel;
            AdsSettings.BannerFromLevel = settings.BannerFromLevel;
        }
    }

    [Serializable]
    public class CampaignSettings
    {
        public string Name;
        public bool EnableBanners = true;
        public int FromLevel = 0;
        public List<TimerData> TimerData = new();
        public int RewardedFromLevel = 0;
        public int BannerFromLevel = 0;
    }
}
