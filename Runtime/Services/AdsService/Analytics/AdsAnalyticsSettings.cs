using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsAnalyticsSettings", fileName = "AdsAnalyticsSettings")]
    public class AdsAnalyticsSettings : ScriptableObject
    {
        public BatchType BatchType;
        public double Threshold;
    }

    public enum BatchType
    {
        None,
        Taichi,
        Once
    }
}