using UnityEngine;
using GoogleMobileAds.Api;
using System;

//GAID Example = 93e4a8ed-f879-4e6d-9ba3-3d83531b8e8b
public class AdNetworkAppLovin : AdNetworkBase
{
    string BannerID, InterstitialID, RewardedID;

    bool BannerCreated;
    bool BannerLoaded;

    int interAttempt, rewardAttempt;

    #region SDK Initialize
    public override void Initialize(bool shouldWaitAppOpen = false)
    {
        if (AdsManager.Instance.TestAds)
        {
            string id = AdsManager.GetAdvertisingID();
            if (id != null)
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[1] { id });
        }

        MaxSdkCallbacks.OnSdkInitializedEvent += MaxSdkCallbacks_OnSdkInitializedEvent;

        if (AdConstants.IsDebugBuild)
            MaxSdk.SetVerboseLogging(true);

        // MaxSdk.SetIsAgeRestrictedUser(AdsManager.Instance.IsForFamily);
        MaxSdk.SetHasUserConsent(ConsentManager.isPersonalized); // for PersonlizedAds
        // MaxSdk.SetSdkKey(AdsManager.Instance.MaxSDKKey);
        MaxSdk.InitializeSdk();
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
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += Interstitial_OnAdHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += Interstitial_OnAdLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Interstitial_OnAdRevenuePaidEvent;

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
            interAttempt = 0;

        return isReady;
    }

    public void ShowInterstitial() // You must check HasInterstitialAd() method before calling this function...
    {
        MaxSdk.ShowInterstitial(InterstitialID);
    }

    void RequestInterstitial()
    {
        MaxSdk.LoadInterstitial(InterstitialID);
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

    private void Interstitial_OnAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            interAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interAttempt));
            Invoke("RequestInterstitial", (float)retryDelay);
        });
    }

    private void Interstitial_OnAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo adInfo)
    {
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