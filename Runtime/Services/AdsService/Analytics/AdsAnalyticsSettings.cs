using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsAnalyticsSettings", fileName = "AdsAnalyticsSettings")]
    public class AdsAnalyticsSettings : ScriptableObject
    {
        public BatchType BatchType;
        public double Threshold;

        public bool EnableMeta = false;
        public BatchType BatchTypeMeta;
        public double ThresholdMeta;

        public bool EnableMetaPurchases = false;
        public bool AddMetaIapBatched = false;
        public bool AddMetaIapLayered = false;
    }

    public enum BatchType
    {
        None,
        Taichi,
        Once
    }
}