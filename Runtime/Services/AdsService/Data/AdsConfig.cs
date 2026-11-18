using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsConfig", fileName = "AdsConfig")]
    public class AdsConfig : ScriptableObject
    {
        public AdsData AppLovin;
        public AdsData Admob;

        public string MeticaApiKey;
        public string MeticaAppId;

        public bool TestMode;
        public bool SkipAds;
        public string PrivacyPolicyUrl;
    }
}
