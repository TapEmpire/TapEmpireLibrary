using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    [Serializable]
    public class AdsAnalyticsSettingsSerializable : RemoteSerializableBase<AdsAnalyticsSettings, AdsAnalyticsSettingsSerializable.AdsAnalyticsRemoteModel>
    {
        public override string TokenName => "AdsAnalyticsSettings";

        public class AdsAnalyticsRemoteModel : IRemoteModel<AdsAnalyticsSettings>
        {
            public BatchType? BatchType;
            public double? Threshold;
            public int? CounterThreshold;
            public bool? AddBanners;
            public bool? AddBannerRevenue;
            public bool? EnableMeta;
            public BatchType? BatchTypeMeta;
            public double? ThresholdMeta;
            public bool? EnableMetaPurchases;
            public bool? AddMetaIapBatched;
            public bool? AddMetaIapLayered;
            public List<RevenueLayer> RevenueLayers;
            public List<RevenueLayer> MetaRevenueLayers;

            public void FromSettings(AdsAnalyticsSettings settings)
            {
                BatchType = settings.BatchType;
                Threshold = settings.Threshold;
                CounterThreshold = settings.CounterThreshold;
                AddBanners = settings.AddBanners;
                AddBannerRevenue = settings.AddBannerRevenue;
                EnableMeta = settings.EnableMeta;
                BatchTypeMeta = settings.BatchTypeMeta;
                ThresholdMeta = settings.ThresholdMeta;
                EnableMetaPurchases = settings.EnableMetaPurchases;
                AddMetaIapBatched = settings.AddMetaIapBatched;
                AddMetaIapLayered = settings.AddMetaIapLayered;
                RevenueLayers = settings.RevenueLayers;
                MetaRevenueLayers = settings.MetaRevenueLayers;
            }

            public void ToSettings(AdsAnalyticsSettings settings)
            {
                settings.BatchType = BatchType ?? settings.BatchType;
                settings.Threshold = Threshold ?? settings.Threshold;
                settings.CounterThreshold = CounterThreshold ?? settings.CounterThreshold;
                settings.AddBanners = AddBanners ?? settings.AddBanners;
                settings.AddBannerRevenue = AddBannerRevenue ?? settings.AddBannerRevenue;
                settings.EnableMeta = EnableMeta ?? settings.EnableMeta;
                settings.BatchTypeMeta = BatchTypeMeta ?? settings.BatchTypeMeta;
                settings.ThresholdMeta = ThresholdMeta ?? settings.ThresholdMeta;
                settings.EnableMetaPurchases = EnableMetaPurchases ?? settings.EnableMetaPurchases;
                settings.AddMetaIapBatched = AddMetaIapBatched ?? settings.AddMetaIapBatched;
                settings.AddMetaIapLayered = AddMetaIapLayered ?? settings.AddMetaIapLayered;

                if (RevenueLayers != null)
                {
                    settings.RevenueLayers = RevenueLayers.ToList();
                }

                if (MetaRevenueLayers != null)
                {
                    settings.MetaRevenueLayers = MetaRevenueLayers.ToList();
                }
            }
        }
    }
}
