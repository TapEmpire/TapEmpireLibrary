using System;
using System.Collections;
using GoogleMobileAds.Api;
using System.Text;
using Firebase.Analytics;
using Io.AppMetrica;
using AdjustSdk;

public class AdData
{
    public string Network;
    public string Mediation;
    public AdFormat Format;
}

public static class AnalyticsManager
{
    // Placement, Network, Mediation, Format, Price, Currency, UnitId
    public static System.Action<string, string, string, AdFormat, double, string, string> OnAdPayed = null;
    public static AdData LastAdData = new AdData();

    static StringBuilder stringBuilder = new StringBuilder();
    public static string PlacementName = "none";

    #region PaidEvents - Firebase , Adjust
    public static void ReportRevenue_Admob(AdValue admobAd, AdImpressionData data)
    {
        if (admobAd == null) return;

        string targetPlacement = isValidPlacementName(data.Format) ? PlacementName : data.Format.ToString();

        double revenue = (admobAd.Value / 1000000.0);
        if (TapEmpire.Services.FirebaseService.IsInitializedDeprecated)
        {
            var impressionParameters = new[] {
            new Parameter("ad_platform", "Admob"),
            new Parameter("ad_source", "Simple Admob"),
            new Parameter("ad_unit_name", $"{data.Format}_{data.AdUnit}"),
            new Parameter("ad_format", "Admob_" + data.Format.ToString()),
            new Parameter("value", revenue),
            new Parameter("currency", admobAd.CurrencyCode),
            (data.Format == AdFormat.Interstitial || data.Format == AdFormat.Rewarded) ? new Parameter("ad_placement", PlacementName.ToLower()) : new Parameter("no_placement", "nothing")
            };
            FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
#if UNITY_IOS
            FirebaseAnalytics.LogEvent("paid_ad_impression", impressionParameters);
#endif
        }

        //Rev Event for Adjust
        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("admob_sdk");
        adjustAdRevenue.SetRevenue(revenue, "USD");
        Adjust.TrackAdRevenue(adjustAdRevenue);

        // GameAnalytics.NewAdEvent(GAAdAction.Show, GetAdType(data.Format), "Admob", targetPlacement);

        //Rev Event for Appmetrica
        AdRevenue rev = new AdRevenue(revenue, "USD");
        rev.AdType = GetAppMetricaAdType(data.Format);
        rev.AdNetwork = "Admob_Native";
        rev.AdUnitId = data.AdUnit;
        if (isValidPlacementName(data.Format))
            rev.AdPlacementName = PlacementName.ToLower();
        AppMetrica.ReportAdRevenue(rev);

        LastAdData.Network = "AdMob";
        LastAdData.Mediation = "AdMob Mediation";
        LastAdData.Format = data.Format;
        OnAdPayed?.Invoke(targetPlacement, LastAdData.Network, LastAdData.Mediation, data.Format, revenue, admobAd.CurrencyCode, data.AdUnit);

        //Rev Event for Appsflyer
        //Dictionary<string, string> dic = new Dictionary<string, string>();
        //dic.Add("ad_format", "admob_" + data.Format.ToString());
        //AppsFlyerAdRevenue.logAdRevenue("simple_admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, revenue, "USD", dic);

        //if (data.Format == AdFormat.Interstitial || data.Format == AdFormat.Rewarded)
        //    SendAppsFlyerEvents();
    }

    public static void ReportRevenue_Applovin(MaxSdkBase.AdInfo maxAd, AdFormat format)
    {
        if (maxAd == null) return;

        string targetPlacement = isValidPlacementName(format) ? PlacementName : format.ToString();

        double revenue = maxAd.Revenue;
        if (TapEmpire.Services.FirebaseService.IsInitializedDeprecated)
        {
            var impressionParameters = new[] {
            new Parameter("ad_platform", "AppLovin"),
            new Parameter("ad_source", maxAd.NetworkName),
            new Parameter("ad_unit_name",$"{format}_{GetMaxNetworkType(maxAd.NetworkName)}_{maxAd.WaterfallInfo.TestName}_{maxAd.AdUnitIdentifier}"),
            new Parameter("ad_format","Applovin_" + format.ToString()),
            new Parameter("value", revenue),
            new Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            (format == AdFormat.Interstitial || format == AdFormat.Rewarded) ? new Parameter("ad_placement", PlacementName.ToLower()) : new Parameter("no_placement", "nothing")
            };

            FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
#if UNITY_IOS
            FirebaseAnalytics.LogEvent("paid_ad_impression", impressionParameters);
#endif
        }

        //Rev Event for Adjust
        AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("applovin_max_sdk");
        adjustAdRevenue.SetRevenue(revenue, "USD");
        adjustAdRevenue.AdRevenueNetwork = maxAd.NetworkName;
        adjustAdRevenue.AdRevenueUnit = $"{format}_{maxAd.AdUnitIdentifier}";
        adjustAdRevenue.AddPartnerParameter("ad_format", maxAd.AdFormat);
        adjustAdRevenue.AddPartnerParameter("ad_unit_id", maxAd.AdUnitIdentifier);
        Adjust.TrackAdRevenue(adjustAdRevenue);

        // GameAnalytics.NewAdEvent(GAAdAction.Show, GetAdType(format), "Max", targetPlacement);

        //Rev Event for Appmetrica
        AdRevenue rev = new AdRevenue(revenue, "USD");
        rev.AdType = GetAppMetricaAdType(format);
        rev.AdNetwork = maxAd.NetworkName;
        rev.AdUnitId = maxAd.AdUnitIdentifier;
        if (isValidPlacementName(format))
            rev.AdPlacementName = PlacementName.ToLower();
        AppMetrica.ReportAdRevenue(rev);

        LastAdData.Network = maxAd.NetworkName;
        LastAdData.Mediation = "Applovin";
        LastAdData.Format = format;
        OnAdPayed?.Invoke(targetPlacement, LastAdData.Network, LastAdData.Mediation, format, revenue, "USD", maxAd.AdUnitIdentifier);

        //Rev Event for Appsflyer
        //Dictionary<string, string> dic = new Dictionary<string, string>();
        //dic.Add("ad_unit_name", maxAd.AdUnitIdentifier);
        //dic.Add("ad_format", "applovin_" + format.ToString());
        //AppsFlyerAdRevenue.logAdRevenue(maxAd.NetworkName, AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax, revenue, "USD", dic);

        //if (format == AdFormat.Interstitial || format == AdFormat.Rewarded)
        //    SendAppsFlyerEvents();
    }

    /*static YandexAppMetricaAdRevenue.AdTypeEnum GetAdType(AdFormat format)
    {
        switch (format)
        {
            case AdFormat.Banner: return YandexAppMetricaAdRevenue.AdTypeEnum.Banner;
            case AdFormat.Interstitial: return YandexAppMetricaAdRevenue.AdTypeEnum.Interstitial;
            case AdFormat.Rewarded: return YandexAppMetricaAdRevenue.AdTypeEnum.Rewarded;
            case AdFormat.AppOpen: return YandexAppMetricaAdRevenue.AdTypeEnum.Other;
            case AdFormat.MREC: return YandexAppMetricaAdRevenue.AdTypeEnum.Mrec; ;
            case AdFormat.NativeAd: return YandexAppMetricaAdRevenue.AdTypeEnum.Native;
            case AdFormat.RewardedInt: return YandexAppMetricaAdRevenue.AdTypeEnum.Interstitial;
            default: return YandexAppMetricaAdRevenue.AdTypeEnum.Other;
        }
    }*/

    static bool isValidPlacementName(AdFormat format)
    {
        return (format == AdFormat.Interstitial || format == AdFormat.Rewarded || format == AdFormat.RewardedInt);
    }

    // static GAAdType GetAdType(AdFormat format)
    // {
    //     switch (format)
    //     {
    //         case AdFormat.Interstitial:
    //             return GAAdType.Interstitial;
    //         case AdFormat.Rewarded:
    //         case AdFormat.RewardedInt:
    //             return GAAdType.RewardedVideo;
    //         case AdFormat.Banner:
    //             return GAAdType.Banner;
    //         case AdFormat.AppOpen:
    //             return GAAdType.OfferWall;
    //         default:
    //             return GAAdType.Undefined;
    //     }
    // }

    static AdType GetAppMetricaAdType(AdFormat format)
    {
        switch (format)
        {
            case AdFormat.Interstitial:
                return AdType.Interstitial;
            case AdFormat.Rewarded:
            case AdFormat.RewardedInt:
                return AdType.Rewarded;
            case AdFormat.Banner:
                return AdType.Banner;
            case AdFormat.AppOpen:
                return AdType.Mrec;
            default:
                return AdType.Other;
        }
    }

    static string GetMaxNetworkType(string NetworkName)
    {
        NetworkName = NetworkName.ToLower();
        switch (NetworkName)
        {
            case "google admob native":
            case "ironsource":
            case "unity ads":
            case "google ad manager":
            case "google ad manager native":
            case "dt exchange":
                return "Waterfall";

            case "google admob":
            case "mintegral":
            case "applovin":
            case "pangle":
            case "applovin_exchange":
            case "liftoff monetize":
            case "inmobi":
            case "smaato":
            case "smaato native":
            case "tapjoy":
            case "adcolony":
            case "facebook":
                return "Bidding";

            default:
                return NetworkName;
        }
    }

    #endregion

    #region PlacementEvents - AppMetrica

    public static void ReportPlacementEvent(AdNetwork network, AdFormat format)
    {
        // string jsonData = $"{{\"{format}\":\"{PlacementName}\"}}";
        // AppMetrica.Instance.ReportEvent("Placements", jsonData);
    }

    #endregion

    #region CustomEvents - Appmetrica

    public static void ReportCustomEvent(AnalyticsType type, params string[] data)
    {
        // if (!data.IsNullOrEmpty())
        // {
        //     ThreadDispatcher.Enqueue(() =>
        //     {
        //         AppMetrica.Instance.ReportEvent(type.ToString(), GetJsonFromParams(data));
        //     });
        // }
    }

    #endregion

    #region Extensions
    static bool IsNullOrEmpty(this string[] value)
    {
        if (value != null)
        {
            return value.Length == 0;
        }
        return true;
    }

    static string GetJsonFromParams(params string[] data)
    {
        stringBuilder.Clear();
        int dataLength = data.Length;
        for (int i = 0; i < dataLength; i++)
        {
            if (i == dataLength - 1)
            {
                stringBuilder.Append($"\"{data[i]}\"");
                break;
            }
            stringBuilder.Append($"{{\"{data[i]}\":");
        }
        stringBuilder.Append('}', data.Length - 1);
        return stringBuilder.ToString();
    }
    #endregion

    #region Appsflyer
    //static int ImpressionCount
    //{
    //    get
    //    {
    //        return PlayerPrefs.GetInt("ImpressionCount", 0);
    //    }
    //    set
    //    {
    //        PlayerPrefs.SetInt("ImpressionCount", value);
    //    }
    //}


    //static TimeSpan TimeSpan()
    //{
    //    DateTime FirstOpen, CurrentDate;
    //    if (!PlayerPrefs.HasKey("FirstOpenDate"))
    //        PlayerPrefs.SetString("FirstOpenDate", DateTime.Now.ToBinary().ToString());

    //    long temp = Convert.ToInt64(PlayerPrefs.GetString("FirstOpenDate"));

    //    FirstOpen = DateTime.FromBinary(temp);
    //    CurrentDate = System.DateTime.Now;
    //    return CurrentDate.Subtract(FirstOpen);
    //}
    //static void SendAppsFlyerEvents()
    //{
    //    if (TimeSpan().TotalDays < 1)
    //    {
    //        ImpressionCount++;
    //        if (ImpressionCount > 5 && ImpressionCount <= 20)
    //            AppsFlyer.sendEvent("af_ad_watched_x" + ImpressionCount.ToString(), null);
    //    }
    //}
    #endregion
}

public enum AnalyticsType { Extras, GameData }
public enum AdNetwork { Admob, Applovin, ironSource }
public enum AdFormat { Banner, MREC, Interstitial, Rewarded, AppOpen, NativeAd, RewardedInt }