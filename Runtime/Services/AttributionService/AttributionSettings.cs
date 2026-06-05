using AdjustSdk;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AttributionSettings", fileName = "AttributionSettings")]
    public class AttributionSettings : ScriptableObject
    {
        public string AppToken;
        public AdjustEnvironment Environment = AdjustEnvironment.Sandbox;
        public AdjustLogLevel LogLevel = AdjustLogLevel.Info;
        public bool SendInBackground;
        public bool LaunchDeferredDeeplink = true;
        public string DefaultTracker;
        public bool CoppaCompliance;
        public bool CostDataInAttribution;
        public bool PreinstallTracking;
        public string PreinstallFilePath;
        public bool AdServices = true;
        public bool IdfaReading = true;
        public bool LinkMe;
        public bool SkanAttribution = true;
    }
}
