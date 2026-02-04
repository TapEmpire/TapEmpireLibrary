using TapEmpire.Services;
using UnityEngine;

namespace TapEmpire.Modules
{
    public static partial class ProgressServiceExtensions
    {
        private static string CampaignKey = "Campaign";

        public static string GetCampaignName(this IProgressService self) => self.GetString(CampaignKey);
        public static void SetCampaignName(this IProgressService self, string value) => self.SetString(CampaignKey, value);
        public static void ClearCampaignName(this IProgressService self) => self.StringValuesDictionary.DeleteKey(CampaignKey);

        public static string GetCampaignNameStatic() => PlayerPrefs.GetString(CampaignKey);
    }
}
