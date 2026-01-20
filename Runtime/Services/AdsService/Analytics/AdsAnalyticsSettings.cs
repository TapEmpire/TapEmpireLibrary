using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/AdsAnalyticsSettings", fileName = "AdsAnalyticsSettings")]
    public class AdsAnalyticsSettings : ScriptableObject
    {
        public BatchType BatchType;
        public double Threshold;

        public int CounterThreshold = 20;
        public bool AddBanners = false;

        public bool EnableMeta = false;
        public BatchType BatchTypeMeta;
        public double ThresholdMeta;

        public bool EnableMetaPurchases = false;
        public bool AddMetaIapBatched = false;
        public bool AddMetaIapLayered = false;

        public List<RevenueLayer> RevenueLayers = new();
        public List<RevenueLayer> MetaRevenueLayers = new();
    }

    public enum BatchType
    {
        None,
        Taichi,
        Once
    }

    [System.Serializable]
    public struct RevenueLayer
    {
        public string Name;
        public float Value;
    }
}