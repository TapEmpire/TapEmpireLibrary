using UnityEngine;
using GoogleMobileAds.Api;
using System;
using Metica.SDK;
using Metica.ADS;
using Cysharp.Threading.Tasks;

//GAID Example = 93e4a8ed-f879-4e6d-9ba3-3d83531b8e8b
public class AdNetworkAppLovin : AdNetworkBase
{
    string BannerID, InterstitialID, RewardedID;

    bool BannerCreated;
    bool BannerLoaded;

    int interstitialRetryAttempt, rewardAttempt;

    public bool IsMeticaAdsEnabled = false;

    #region SDK Initialize
    public override async UniTask Initialize(bool shouldWaitAppOpen = false)
    {
        if (AdsManager.Instance.TestAds)
        {
            string id = AdsManager.GetAdvertisingID();
            if (id != null)
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[1] { id });
        }

        await InitializeMetica();

        MaxSdkCallbacks.OnSdkInitializedEvent += MaxSdkCallbacks_OnSdkInitializedEvent;

        if (AdConstants.IsDebugBuild)
            MaxSdk.SetVerboseLogging(true);

        // MaxSdk.SetIsAgeRestrictedUser(AdsManager.Instance.IsForFamily);
        MaxSdk.SetHasUserConsent(ConsentManager.isPersonalized); // for PersonlizedAds
        // MaxSdk.SetSdkKey(AdsManager.Instance.MaxSDKKey);
        MaxSdk.InitializeSdk();
    }

    public async UniTask InitializeMetica()
    {
        MeticaSdk.CurrentUserId = "your_unique_user_id";

        var meticaConfiguration = new MeticaConfiguration();

        IsMeticaAdsEnabled = await MeticaAds.InitializeAsync(meticaConfiguration);
    }

    void OnDisable()
    {
        if (isInitialized)
            MaxSdkCallbacks.OnSdkInitializedEvent -= MaxSdkCallbacks_OnSdkInitializedEvent;
    }

    private void MaxSdkCallbacks_OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration obj)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            isInitialized = true;
            AssignIDs();

            InitializeRewardedAds();
            if (!AdsManager.Instance.AreAdsRemoved)
                InitializeInterstitialAds();

            // GameAnalyticsSDK.GameAnalyticsILRD.SubscribeMaxImpressions();
            AdsManager.OnApplovinInitSuccess?.Invoke();
        });
    }

    void AssignIDs()
    {
        InterstitialID = AdsManager.Instance.MaxInterstitial;
        RewardedID = AdsManager.Instance.MaxRewarded;
        BannerID = AdsManager.Instance.MaxBanner;
    }

    #endregion

    #region Banner Ad

    public void InitializeBannerAds()
    {
        if (!BannerCreated)
        {
            MaxSdk.CreateBanner(BannerID, AdsManager.Instance.MaxBannerPos);
            MaxSdk.StartBannerAutoRefresh(BannerID);

            // MaxSdk.SetBannerExtraParameter(BannerID, "ad-refresh-rate", AdsManager.Instance.BannerRefreshRate.ToString());
            MaxSdk.SetBannerExtraParameter(BannerID, "adaptive_banner", "false");
            MaxSdk.SetBannerExtraParameter(BannerID, "collapsible", "none");
            MaxSdk.SetBannerBackgroundColor(BannerID, Color.black);

            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += Banner_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += Banner_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

            BannerCreated = true;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                if (AdsManager.Instance.BannerSize == BannerWidth.Half)
                    MaxSdk.SetBannerWidth(BannerID, 320);
            }
        }
    }

    public bool HasBanner()
    {
        if (BannerCreated)
            return BannerLoaded;
        else
        {
            if (isInitialized)
                InitializeBannerAds();
            return false;
        }
    }

    public void ShowBanner()
    {
        MaxSdk.ShowBanner(BannerID);
    }

    public void HideBanner()
    {
        if (isInitialized && BannerCreated)
            MaxSdk.HideBanner(BannerID);
    }

    public void DestroyBanner()
    {
        if (!isInitialized) return;

        BannerLoaded = false;

        if (BannerCreated)
        {
            BannerCreated = false;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= Banner_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= Banner_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerAdRevenuePaidEvent;

            MaxSdk.DestroyBanner(BannerID);
        }
    }

    private void Banner_OnAdLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            BannerLoaded = true;
            AdsManager.Instance.OnMaxBannerLoaded?.Invoke();
        });
    }

    private void Banner_OnAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            BannerLoaded = false;
            AdsManager.Instance.OnMaxBannerFailed?.Invoke();
        });
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Applovin(adInfo, AdFormat.Banner);
        });
    }

    #endregion

    #region Interstitial Ad

    void InitializeInterstitialAds()
    {
        // MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += Interstitial_OnAdHiddenEvent;
        // MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += Interstitial_OnAdLoadFailedEvent;
        // MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Interstitial_OnAdRevenuePaidEvent;

        if (IsMeticaAdsEnabled)
        {
            MeticaAdsCallbacks.Interstitial.OnAdLoadSuccess += (meticaAd) => Interstitial_OnLoadedEvent(meticaAd.adUnitId, meticaAd.ToAdInfo());
            MeticaAdsCallbacks.Interstitial.OnAdLoadFailed += (error) => Interstitial_OnAdLoadFailedEvent("", null);
            MeticaAdsCallbacks.Interstitial.OnAdShowFailed += (meticaAd, error) => Interstitial_OnFailedToDisplayEvent(meticaAd.adUnitId, error);
            MeticaAdsCallbacks.Interstitial.OnAdHidden += (meticaAd) => Interstitial_OnAdHiddenEvent(meticaAd.adUnitId, meticaAd.ToAdInfo());
            MeticaAdsCallbacks.Interstitial.OnAdRevenuePaid += (meticaAd) => Interstitial_OnAdRevenuePaidEvent(meticaAd.adUnitId, meticaAd.ToAdInfo());
        }
        else
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += Interstitial_OnLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (adUnitId, adInfo) => Interstitial_OnAdLoadFailedEvent(adUnitId, adInfo.Message);
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (adUnitId, errorInfo, adInfo) => Interstitial_OnFailedToDisplayEvent(adUnitId, errorInfo.Message);
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += Interstitial_OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Interstitial_OnAdRevenuePaidEvent;
        }

        RequestInterstitial();
    }

    public override bool HasInterstitial(bool doRequest)
    {
        if (!isInitialized)
            return false;

        bool isReady = MaxSdk.IsInterstitialReady(InterstitialID);
        if (!isReady && doRequest)
            RequestInterstitial();

        if (isReady)
            interstitialRetryAttempt = 0;

        return isReady;
    }

    public void ShowInterstitial() // You must check HasInterstitialAd() method before calling this function...
    {
        MaxSdk.ShowInterstitial(InterstitialID);
    }

    void RequestInterstitial()
    {
        if (IsMeticaAdsEnabled)
        {
            MeticaAds.LoadInterstitial();
        }
        else
        {
            MeticaAds.NotifyAdLoadAttempt(InterstitialID);
            MaxSdk.LoadInterstitial(InterstitialID);
        }
    }

    private void Interstitial_OnLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        if (!IsMeticaAdsEnabled)
        {
            MeticaAds.NotifyAdLoadSuccess(adInfo.ToMeticaAd());
        }

        interstitialRetryAttempt = 0;
    }

    private void Interstitial_OnAdLoadFailedEvent(string adUnitId, string error)
    {
        if (!IsMeticaAdsEnabled)
        {
            MeticaAds.NotifyAdLoadFailed(adUnitId, error);
        }

        ThreadDispatcher.Enqueue(() =>
        {
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
            Invoke("RequestInterstitial", (float)retryDelay);
        });
    }

    private void Interstitial_OnFailedToDisplayEvent(string adUnitId, string error)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            RequestInterstitial();
        });
    }

    private void Interstitial_OnAdHiddenEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportPlacementEvent(AdNetwork.Applovin, AdsManager.Instance.InterstitialType);
            AdsManager.Instance.InvokeReward();
            RequestInterstitial();
        });
    }

    private void Interstitial_OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo adInfo)
    {
        if (!IsMeticaAdsEnabled)
        {
            MeticaAds.NotifyAdShowSuccess(adInfo.ToMeticaAd());
        }

        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Applovin(adInfo, AdFormat.Interstitial);
        });
    }

    #endregion

    #region Rewarded Ad

    void InitializeRewardedAds()
    {
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += Rewarded_OnAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += Rewarded_OnAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += Rewarded_OnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += Rewarded_OnAdReceivedRewardEvent;

        RequestRewardedAd();
    }

    public void ShowRewardedAd() //You must check HasRewardedAd() method before calling this function...
    {
        MaxSdk.ShowRewardedAd(RewardedID);
    }

    public override bool HasRewarded(bool doRequest)
    {
        if (!isInitialized)
            return false;

        bool isReady = MaxSdk.IsRewardedAdReady(RewardedID);
        if (!isReady && doRequest)
            RequestRewardedAd();

        if (isReady)
            rewardAttempt = 0;

        return isReady;
    }

    void RequestRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedID);
    }

    private void Rewarded_OnAdHiddenEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportPlacementEvent(AdNetwork.Applovin, AdFormat.Rewarded);
            RequestRewardedAd();
        });
    }

    private void Rewarded_OnAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            rewardAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardAttempt));
            Invoke("RequestRewardedAd", (float)retryDelay);
        });
    }

    private void Rewarded_OnAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AdsManager.Instance.InvokeReward();
        });
    }

    private void Rewarded_OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo adInfo)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Applovin(adInfo, AdFormat.Rewarded);
        });
    }

    #endregion
}