using UnityEngine;
using GoogleMobileAds.Api;
using System;
using R3;
using Cysharp.Threading.Tasks;
using Firebase.Crashlytics;

public class AdNetworkAdmob : AdNetworkBase
{
    public ReactiveProperty<bool> ShouldWaitAppOpen = new ReactiveProperty<bool>(true);
    public bool CanShowAppOpen => isInitialized && !m_IsShowingAppOpenAd && appOpenAd != null;

    #region Fields & Properties
    BannerView bannerView, mrecView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
    AppOpenAd appOpenAd;

    bool m_IsShowingAppOpenAd;
    bool BannerStatus;

    bool LoadingAppOpen;
    [System.NonSerialized] public bool ShowAppOpenOnLoad;
    AdImpressionData bannerImp, mrecImp, interImp, rewardedImp, appOpenImp;

    string interAdUnitId;
    string rewardAdUnitId;

    private IDisposable _subscription = null;

    #endregion

    #region SDK Initialize
    public override async UniTask Initialize(bool shouldWaitAppOpen = false)
    {
        if (ShouldWaitAppOpen.Value == true)
        {
            ShouldWaitAppOpen.Value = shouldWaitAppOpen;
        }
        MobileAds.SetiOSAppPauseOnBackground(true);
        //SetConfigurations();
        MobileAds.RaiseAdEventsOnUnityMainThread = false;
        MobileAds.Initialize(HandleInitCompleteAction);
        AppStateEventNotifier.AppStateChanged += AdsManager.Instance.OnAppStateChanged;

        await UniTask.CompletedTask;
    }

    public void OnRelease()
    {
        AppStateEventNotifier.AppStateChanged -= AdsManager.Instance.OnAppStateChanged;
        _subscription?.Dispose();
    }


    void SetConfigurations()
    {
        RequestConfiguration config = new RequestConfiguration()
        {
            TagForUnderAgeOfConsent = AdsManager.Instance.IsForFamily ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False,
        };

        MobileAds.SetRequestConfiguration(config);
    }

    private void HandleInitCompleteAction(InitializationStatus status)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            isInitialized = true;
            RequestAds();
        });
    }

    private void RequestAds()
    {
        // Probably issues with threads, but the callback is launched before the _subscription is assigned.
        bool shouldCleanupSubscription = false;
        _subscription = ShouldWaitAppOpen.Subscribe(value =>
        {
            if (!value && !shouldCleanupSubscription)
            {
                RequestVideoAds();
                shouldCleanupSubscription = true;

                _subscription?.Dispose();
                _subscription = null;
                //Весь код вставлять выше _subscription.Dispose(); все, что ниже не работает
            }
        });

        if (!AdsManager.Instance.AreAdsRemoved && !AdsManager.Instance.IsForFamily)
        {
            RequestAppOpenAd();
        }

        AdsManager.OnAdmobInitSuccess?.Invoke();
    }

    private void RequestVideoAds()
    {
        if (!AdsManager.Instance.AreAdsRemoved)
        {
            RequestInterstitial();
        }

        RequestRewardedAd();
    }

    #endregion

    #region Banner Ad
    public void ShowBanner()
    {
        if (!isInitialized) return;

        BannerStatus = true;
        if (bannerView != null)
        {
            bannerView.Show();
            UpdateBannerLayout("ShowBanner");
        }
        else
            RequestBanner();
    }

    void RequestBanner()
    {
        string adUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.Banner) : AdsManager.Instance.BannerID;
        if (string.IsNullOrEmpty(adUnitId)) return;

        int width = (AdsManager.Instance.BannerSize == BannerWidth.Full) ? AdSize.FullWidth : 320;
        // AdSize adSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);
        AdSize adSize = new AdSize(width, 0);

        if (bannerView != null)
            bannerView.Destroy();

        bannerImp = new AdImpressionData(adUnitId, AdFormat.Banner);
        bannerView = new BannerView(adUnitId, adSize, AdsManager.Instance.BannerPos);
        // GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, bannerView);

        bannerView.OnBannerAdLoaded += BannerView_OnBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += BannerView_OnBannerAdLoadFailed;
        bannerView.OnAdPaid += BannerView_OnAdPaid;
        bannerView.LoadAd(CreateAdRequest());

    }

    private void BannerView_OnBannerAdLoaded()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            UpdateBannerLayout("OnAdLoaded");
            if (!BannerStatus)
                HideBanner();
        });
    }

    private void BannerView_OnBannerAdLoadFailed(LoadAdError obj)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            DestroyBanner();
        });
    }

    private void BannerView_OnAdPaid(AdValue value)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Admob(value, bannerImp);
        });
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.Hide();

        BannerStatus = false;
    }

    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
        BannerStatus = false;
    }

    private void UpdateBannerLayout(string context)
    {
        if (!isInitialized || bannerView == null)
            return;

        try
        {
            float width = bannerView.GetWidthInPixels();
            float height = bannerView.GetHeightInPixels();
            var size = new Vector2(width, height);

            Debug.Log($"[Admob Banner] {context} - Size: {width}x{height}");
            AdsManager.Instance.BannerLayout.Value = size;
        }
        catch (Exception e)
        {
            Crashlytics.LogException(e);
        }
    }

    #endregion

    #region MREC Ad

    public void ShowMREC()
    {
        if (!isInitialized) return;

        if (mrecView != null)
            mrecView.Show();
        else
            RequestMREC();
    }
    
    public void ShowMREC(int x, int y)
    {
        if (!isInitialized) return;

        if (mrecView != null)
            mrecView.Show();
        else
            RequestMREC(x, y);
    }

    void RequestMREC()
    {
        string adUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.Banner) : AdsManager.Instance.MrecID;
        if (string.IsNullOrEmpty(adUnitId)) return;

        if (mrecView != null)
            mrecView.Destroy();

        mrecImp = new AdImpressionData(adUnitId, AdFormat.MREC);
        mrecView = new BannerView(adUnitId, new AdSize(300, 250), AdsManager.Instance.MrecPos);
        // GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, mrecView);

        mrecView.OnBannerAdLoadFailed += MrecView_OnBannerAdLoadFailed;
        mrecView.OnBannerAdLoaded += MrecView_OnBannerAdLoaded;
        mrecView.OnAdPaid += MrecView_OnAdPaid;
        mrecView.LoadAd(CreateAdRequest());
    }
    
    void RequestMREC(int x, int y)
    {
        string adUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.Banner) : AdsManager.Instance.MrecID;
        if (string.IsNullOrEmpty(adUnitId)) return;

        if (mrecView != null)
            mrecView.Destroy();

        mrecImp = new AdImpressionData(adUnitId, AdFormat.MREC);
        mrecView = new BannerView(adUnitId, new AdSize(300, 250), x, y);

        mrecView.OnBannerAdLoadFailed += MrecView_OnBannerAdLoadFailed;
        mrecView.OnBannerAdLoaded += MrecView_OnBannerAdLoaded;
        mrecView.OnAdPaid += MrecView_OnAdPaid;
        mrecView.LoadAd(CreateAdRequest());
    }

    private void MrecView_OnBannerAdLoaded()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            if (!AdsManager.Instance.MrecStatus)
                HideMREC();
        });
    }

    private void MrecView_OnBannerAdLoadFailed(LoadAdError obj)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AdsManager.Instance.DestroyMREC();
        });
    }

    private void MrecView_OnAdPaid(AdValue value)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Admob(value, mrecImp);
        });
    }

    public void HideMREC()
    {
        if (mrecView != null)
            mrecView.Hide();
    }

    public void DestroyMREC()
    {
        if (mrecView != null)
        {
            mrecView.Destroy();
            mrecView = null;
        }
    }

    #endregion

    #region Interstitial Ad
    public void ShowInterstitial()
    {
        interstitialAd.Show();
    }

    public override bool HasInterstitial(bool doRequest)
    {
        if (!isInitialized) return false;

        if (interstitialAd != null && interstitialAd.CanShowAd())
            return true;
        else
        {
            if (doRequest)
                RequestInterstitial();
            return false;
        }
    }

    void RequestInterstitial()
    {
        interAdUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.Interstitial) : AdsManager.Instance.InterstitialID;
        if (string.IsNullOrEmpty(interAdUnitId)) return;

        if (interstitialAd != null)
            interstitialAd.Destroy();

        interImp = new AdImpressionData(interAdUnitId, AdFormat.Interstitial);
        InterstitialAd.Load(interAdUnitId, CreateAdRequest(), InterstitialLoadCallback);
    }

    void InterstitialLoadCallback(InterstitialAd ad, LoadAdError loadError)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            if (loadError == null && ad != null) // Success
            {
                interstitialAd = ad;
                interstitialAd.OnAdPaid += InterstitialAd_OnAdPaid;
                interstitialAd.OnAdImpressionRecorded += InterstitialAd_OnAdImpressionRecorded;
                interstitialAd.OnAdFullScreenContentClosed += InterstitialAd_OnAdFullScreenContentClosed;
                // GameAnalyticsILRD.SubscribeAdMobImpressions(interAdUnitId, interstitialAd);
            }
            //else Failed 
        });
    }

    private void InterstitialAd_OnAdPaid(AdValue value)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Admob(value, interImp);
        });
    }

    private void InterstitialAd_OnAdImpressionRecorded()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportPlacementEvent(AdNetwork.Admob, AdsManager.Instance.InterstitialType);
        });
    }

    private void InterstitialAd_OnAdFullScreenContentClosed()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AdsManager.Instance.InvokeReward();
            RequestInterstitial();
        });
    }

    #endregion

    #region Rewarded Ad

    public void ShowRewardedAd()
    {
        rewardedAd.Show(RewardedAd_OnUserEarnedReward);
    }

    public override bool HasRewarded(bool doRequest)
    {
        if (!isInitialized) return false;

        if (rewardedAd != null && rewardedAd.CanShowAd())
            return true;
        else
        {
            if (doRequest)
                RequestRewardedAd();
            return false;
        }
    }

    public void RequestRewardedAd()
    {
        rewardAdUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.Rewarded) : AdsManager.Instance.RewardedID;
        if (string.IsNullOrEmpty(rewardAdUnitId)) return;

        if (rewardedAd != null)
            rewardedAd.Destroy();

        rewardedImp = new AdImpressionData(rewardAdUnitId, AdFormat.Rewarded);
        RewardedAd.Load(rewardAdUnitId, CreateAdRequest(), RewardedLoadCallback);
    }

    void RewardedLoadCallback(RewardedAd ad, LoadAdError loadError)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            if (loadError == null && ad != null) // Success
            {
                rewardedAd = ad;
                rewardedAd.OnAdPaid += RewardedAd_OnAdPaid;
                rewardedAd.OnAdImpressionRecorded += RewardedAd_OnAdImpressionRecorded;
                rewardedAd.OnAdFullScreenContentClosed += RewardedAd_OnAdFullScreenContentClosed;
                // GameAnalyticsILRD.SubscribeAdMobImpressions(rewardAdUnitId, rewardedAd);
            }
            //else Failed
        });
    }

    private void RewardedAd_OnAdPaid(AdValue value)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Admob(value, rewardedImp);
        });
    }

    private void RewardedAd_OnAdImpressionRecorded()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportPlacementEvent(AdNetwork.Admob, AdFormat.Rewarded);
        });
    }

    private void RewardedAd_OnAdFullScreenContentClosed()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            RequestRewardedAd();
        });
    }

    private void RewardedAd_OnUserEarnedReward(Reward e)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AdsManager.Instance.InvokeReward();
        });
    }

    #endregion

    #region AppOpen

    public void RequestAppOpenAd()
    {
        string adUnitId = AdsManager.Instance.TestAds ? AdConstants.GetAdmobTestID(AdFormat.AppOpen) : AdsManager.Instance.AppOpenID;
        if (string.IsNullOrEmpty(adUnitId)) return;

        LoadingAppOpen = true;
        appOpenImp = new AdImpressionData(adUnitId, AdFormat.AppOpen);
        if (appOpenAd != null)
            appOpenAd.Destroy();

        // AppOpenAd.Load(adUnitId, Screen.orientation, CreateAdRequest(), AppOpenResponse);
        AppOpenAd.Load(adUnitId, CreateAdRequest(), AppOpenResponse);
    }

    public void AppOpenResponse(AppOpenAd ad, LoadAdError error)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            LoadingAppOpen = false;
            if (error == null && ad != null)
            {
                appOpenAd = ad;
                appOpenAd.OnAdFullScreenContentOpened += AppOpenAd_OnAdFullScreenContentOpened;
                appOpenAd.OnAdFullScreenContentClosed += AppOpenAd_OnAdFullScreenContentClosed;
                appOpenAd.OnAdFullScreenContentFailed += AppOpenAd_OnAdFullScreenContentFailed;
                appOpenAd.OnAdPaid += AppOpenAd_OnAdPaid;
                // GameAnalyticsILRD.SubscribeAdMobImpressions(AdsManager.Instance.AppOpenID, appOpenAd);

                ShouldWaitAppOpen.Value = false;

                // if (ShowAppOpenOnLoad)
                // {
                //     ShowAppOpenOnLoad = false;
                //     ShowAppOpen();
                // }
            }
            else
                DestroyAppOpen();
        });
    }

    private void AppOpenAd_OnAdFullScreenContentOpened()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            m_IsShowingAppOpenAd = true;
        });
    }

    private void AppOpenAd_OnAdFullScreenContentClosed()
    {
        ThreadDispatcher.Enqueue(() =>
        {
            DestroyAppOpen();
            RequestAppOpenAd();

            AdsManager.Instance.ResumeAllBanners();
            AdsManager.Instance.AppOpenShown();
        });
    }

    private void AppOpenAd_OnAdFullScreenContentFailed(AdError obj)
    {
        ThreadDispatcher.Enqueue(DestroyAppOpen);
    }

    private void AppOpenAd_OnAdPaid(AdValue value)
    {
        ThreadDispatcher.Enqueue(() =>
        {
            AnalyticsManager.ReportRevenue_Admob(value, appOpenImp);
        });
    }

    public void ShowAppOpen()
    {
        if (!isInitialized || m_IsShowingAppOpenAd) return;

        if (appOpenAd != null)
        {
            AdsManager.Instance.HideAllBanners();
            appOpenAd.Show();
        }
        else if (!LoadingAppOpen)
            RequestAppOpenAd();
    }

    void DestroyAppOpen()
    {
        m_IsShowingAppOpenAd = false;

        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }
    }

    #endregion

    #region Others
    public AdRequest CreateAdRequest()
    {
        var adRequest = new AdRequest();
        adRequest.Extras.Add("npa", ConsentManager.isPersonalized ? "0" : "1");
        adRequest.Extras.Add("collapsible", "0");
        return adRequest;
    }

    #endregion
}