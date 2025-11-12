using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsAnalyticsSettings", fileName = "AdsAnalyticsSettings")]
    public class AdsAnalyticsSettings : ScriptableObject
    {
        public BatchType BatchType;
        public double Threshold;

        public BatchType BatchTypeMeta;
        public double ThresholdMeta;

        public bool EnableMetaPurchases = false;
    }

    public enum BatchType
    {
        None,
        Taichi,
        Once
    }
}