using UnityEngine;

namespace TapEmpire.Experimental
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsConfig", fileName = "AdsConfig")]
    public class AdsConfig : ScriptableObject
    {
        public AdsData AppLovin;
        public AdsData Admob;

        public bool TestMode;
        public string PrivacyPolicyUrl;
    }
}
