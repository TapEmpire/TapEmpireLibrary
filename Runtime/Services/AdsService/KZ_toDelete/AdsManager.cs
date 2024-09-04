using System;
using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using com.adjust.sdk;
using TapEmpire.Utility;
using System.Threading;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

[HideMonoScript]
public class AdsManager : MonoBehaviour
{
    [ShowInInspector, DisplayAsString, GUIColor(1.0f, 1.0f, 1.0f), BoxGroup("Info", false)]
    public const string Version = "2.5.2";

    [ShowInInspector, DisplayAsString, GUIColor(1.0f, 1.0f, 1.0f), BoxGroup("Info", false)]
    public const string Release_Date = "March 11, 2024";

    #region Instance
    public static AdsManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.unityLogger.logEnabled = true; // AdConstants.IsDebugBuild;
        }
        else
            Destroy(gameObject);
    }

    #endregion

    #region Fields 

    [Header("Settings")]
    public bool IsForFamily = false;
    public bool TestAds = false;
    public bool EnableAppOpen = true;

    [Header("References")]
    [SerializeField] AdNetworkAdmob Admob;
    [SerializeField] AdNetworkAppLovin Applovin;
    [SerializeField] FetchAdsRemoteSettings RemoteSettings;

    [Header("Admob IDs")]
    public string AppID;
    public string BannerID, MrecID, InterstitialID, RewardedID, AppOpenID;

    [Header("AppLovin IDs")]
    public string MaxSDKKey;
    public string MaxBanner;
    public string MaxInterstitial;
    public string MaxRewarded;

    [Header("Banner Settings")]
    public BannerWidth BannerSize = BannerWidth.Full;
    public AdPosition BannerPos = AdPosition.Bottom;
    public AdPosition MrecPos = AdPosition.BottomLeft;

    [Header("Links")]
    [SerializeField] string PrivacyPolicy;
    public string MoreGames;

    [HideInInspector] public bool BannerStatus = false;
    [HideInInspector] public bool MrecStatus = false;
    bool BannerWasActive = false;
    bool MrecWasActive = false;

    #endregion

    #region Properties

    public string RateUsLink => "https://play.google.com/store/apps/details?id=" + Application.identifier;
    public bool HasInterstitial => (Applovin.HasInterstitial(false) || Admob.HasInterstitial(false)) && ReadyForNextInterstitial;
    public bool HasAnyRewarded => (Applovin.HasRewarded(false) || Admob.HasRewarded(false) || Applovin.HasInterstitial(false) || Admob.HasInterstitial(false));
    public bool HasRewarded => (Applovin.HasRewarded(false) || Admob.HasRewarded(false));
    public AdFormat InterstitialType => (OnRewardComplete != null) ? AdFormat.RewardedInt : AdFormat.Interstitial;
    public Action OnMaxBannerLoaded { get; private set; }
    public Action OnMaxBannerFailed { get; private set; }
    public Action<bool> OnConsentObtained = null;

    public static bool IsAdmobInitSuccess { get; private set; }
    public static bool IsApplovinInitSuccess { get; private set; }

    Action OnRewardComplete;

    public MaxSdkBase.BannerPosition MaxBannerPos
    {
        get
        {
            switch (BannerPos)
            {
                case AdPosition.Top: return MaxSdkBase.BannerPosition.TopCenter;
                case AdPosition.Bottom: return MaxSdkBase.BannerPosition.BottomCenter;
                case AdPosition.TopLeft: return MaxSdkBase.BannerPosition.TopLeft;
                case AdPosition.TopRight: return MaxSdkBase.BannerPosition.TopRight;
                case AdPosition.BottomLeft: return MaxSdkBase.BannerPosition.BottomLeft;
                case AdPosition.BottomRight: return MaxSdkBase.BannerPosition.BottomRight;
                case AdPosition.Center: return MaxSdkBase.BannerPosition.Centered;
                default: return MaxSdkBase.BannerPosition.BottomCenter;
            }
        }
    }

    #endregion

    #region Delegates

    public delegate void OnAdnetworkInit();
    public static OnAdnetworkInit OnAdmobInitSuccess;
    public static OnAdnetworkInit OnApplovinInitSuccess;

    #endregion

    #region Initialization

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ThreadDispatcher.Initialize();
        AdsRemoteSettings.Initialize();
        GameAnalyticsSDK.GameAnalytics.Initialize();
    }

    public async UniTask Initialize_AdNetworks()
    {
        await Initialize();
    }

    public void OnRelease()
    {
        Admob.OnRelease();
    }

    private async UniTask Initialize()
    {
        OnAdmobInitSuccess = () => { IsAdmobInitSuccess = true; };
        OnApplovinInitSuccess = () => { IsApplovinInitSuccess = true; };

        await UniTask.WaitUntil(() => AdConstants.InternetAvailable);
        await Retry_Consent();

        OnConsentObtained?.Invoke(ConsentManager.isPersonalized);

        PassAdjustConsentParameters();

        await UniTask.WaitForSeconds(2);

        Admob.Initialize();
        await UniTask.WaitUntil(() => IsAdmobInitSuccess);


        #region Applovin

        if (Ram() <= 2)
        {
            if (AdsRemoteSettings.Instance.ShowMediationOn2GB)
                Applovin.Initialize();
        }
        else
            Applovin.Initialize();

        await UniTask.WaitUntil(() => IsApplovinInitSuccess);

        SubscribeToMaxBanners();

        #endregion

        ReportDeviceInfo();
        OnBarrierDone();
    }

    private async UniTask Retry_Consent()
    {
        ConsentManager.GatherConsent(TestAds, IsForFamily,
            (status, message) =>
            {
                ThreadDispatcher.Enqueue(() =>
                {
                    MonetizationLogger.Log($"Retry UMP Response | Status : {status}, Message : {message}");
                });
            });

        if (ConsentManager.ConsentStillRequired)
        {
            await UniTask.WaitWhile(() => ConsentManager.IsFetching);
        }
    }

    private void SubscribeToMaxBanners()
    {
        OnMaxBannerLoaded = delegate
        {
            Admob.HideBanner();
            if (BannerStatus)
                Applovin.ShowBanner();
            else
                Applovin.HideBanner();
        };

        OnMaxBannerFailed = delegate
        {
            Applovin.HideBanner(); // Max banner does not hides when it fails to load new one!
            if (BannerStatus)
                Admob.ShowBanner();
            else
                Admob.HideBanner();
        };
    }

    private void OnBarrierDone()
    {
        UniTaskUtility.ExecuteNextFrame(() => global::AdsManager.Instance.ShowBanner(), CancellationToken.None).Forget();
    }

    void ReportDeviceInfo()
    {
        int DeviceRam = Mathf.Clamp(Mathf.CeilToInt(Ram()), 0, 8);
        string RamInGB = $"{DeviceRam.ToString()}GB";
        //AnalyticsManager.ReportCustomEvent(AnalyticsType.Extras, "Ram", RamInGB);
        if (ConsentManager.isEurArea)
            AnalyticsManager.ReportCustomEvent(AnalyticsType.Extras, "Consent", ConsentManager.isPersonalized ? "Accepted" : "Denied");

        //GameAnalyticsSDK.GameAnalytics.NewDesignEvent($"{AnalyticsType.Extras.ToString()}:Ram:{RamInGB}");
        GameAnalyticsSDK.GameAnalytics.SetCustomDimension01(RamInGB);
        GameAnalyticsSDK.GameAnalytics.SetCustomDimension02(ConsentInformation.ConsentStatus.ToString());
    }
    #endregion

    #region Banner

    public void ShowBanner()
    {
        if (!AdConstants.AdsRemoved)
        {
            BannerStatus = true;
            if (Applovin.HasBanner())
                Applovin.ShowBanner();
            else
                Admob.ShowBanner();
        }
    }

    public void HideBanner()
    {
        BannerStatus = false;
        Applovin.HideBanner();
        Admob.HideBanner();
    }

    public void DestroyBanner()
    {
        BannerStatus = false;
        Applovin.DestroyBanner();
        Admob.DestroyBanner();
    }

    public void RepositionBanner(AdPosition pos, BannerWidth width)
    {
        if (BannerPos != pos || BannerSize != width)
        {
            BannerPos = pos;
            BannerSize = width;
            Applovin.DestroyBanner();
            Admob.DestroyBanner();
        }
    }

    #endregion

    #region MREC
    public void ShowMREC()
    {
        if (!AdConstants.AdsRemoved)
        {
            MrecStatus = true;
            Admob.ShowMREC();
        }
    }

    public void HideMREC()
    {
        MrecStatus = false;
        Admob.HideMREC();
    }

    public void DestroyMREC()
    {
        MrecStatus = false;
        Admob.DestroyMREC();
    }

    public void RepositionMREC(AdPosition newPos)
    {
        if (MrecPos != newPos)
        {
            this.MrecPos = newPos;
            DestroyMREC();
        }
    }
    #endregion

    #region Banners Status

    public void HideAllBanners()
    {
        BannerWasActive = BannerStatus;
        MrecWasActive = MrecStatus;
        HideBanner();
        HideMREC();
    }

    public void ResumeAllBanners()
    {
        if (BannerWasActive)
            ShowBanner();

        if (MrecWasActive)
            ShowMREC();
    }

    #endregion

    #region Interstital

    public void ShowInterstitial(string placementName)
    {
        AnalyticsManager.PlacementName = placementName;
        OnRewardComplete = null;
        ShowInterstitial();
    }

    public void ShowInterstitial(Action onReward, string placementName)
    {
        AnalyticsManager.PlacementName = placementName;
        OnRewardComplete = onReward;
        ResetInterstitialTime();
        ShowInterstitial();
    }

    void ShowInterstitial()
    {
        if (!AdConstants.AdsRemoved && HasInterstitial)
        {
            ExtendAppOpenTime();
            ExtendInterstitialTime();

            if (Applovin.HasInterstitial(true)) { Applovin.ShowInterstitial(); }
            else if (Admob.HasInterstitial(true)) { Admob.ShowInterstitial(); }
        }
    }

    float InterstitialTimer = 0;
    bool ReadyForNextInterstitial => true; // Time.time > InterstitialTimer;
    public void ExtendInterstitialTime()
    {
        InterstitialTimer = Time.time + AdsRemoteSettings.Instance.NextInterstitialDelay;
    }
    public void ResetInterstitialTime()
    {
        InterstitialTimer = 0;
    }

    #region Static Interstitial
    public void ShowInterstitial_Static(string placementName)
    {
        AnalyticsManager.PlacementName = placementName;

        if (!AdConstants.AdsRemoved && ReadyForNextInterstitial)
        {
            if (Admob.HasInterstitial(true))
            {
                ExtendAppOpenTime();
                ExtendInterstitialTime();
                Admob.ShowInterstitial();
            }
        }
    }

    public void ShowInterstitial_Static(Action onReward, string placementName)
    {
        AnalyticsManager.PlacementName = placementName;
        OnRewardComplete = onReward;

        if (!AdConstants.AdsRemoved && Admob.HasInterstitial(true))
        {
            ExtendAppOpenTime();
            ExtendInterstitialTime();
            Admob.ShowInterstitial();
        }
    }
    #endregion

    #endregion

    #region Rewarded

    public void ShowRewarded(Action UserReward, string placementName)
    {
        if (!AdConstants.InternetAvailable)
        {
            MobileToast.Show("Sorry, No Internet Connection!", true);
            return;
        }

        OnRewardComplete = UserReward;
        AnalyticsManager.PlacementName = placementName;

        if (Applovin.HasRewarded(true)) { ExtendAppOpenTime(); ExtendInterstitialTime(); Applovin.ShowRewardedAd(); }
        else if (Admob.HasRewarded(true)) { ExtendAppOpenTime(); ExtendInterstitialTime(); Admob.ShowRewardedAd(); }
        else ShowInterstitial(UserReward, placementName);
    }

    public void InvokeReward()
    {
        OnRewardComplete?.Invoke();
        OnRewardComplete = null;
    }
    #endregion

    #region AppOpen

    float AppOpenTimer = 0;
    int NextAppOpenDelay = 5;

    public void SetAppOpenAutoShow(bool value)
    {
        Admob.ShowAppOpenOnLoad = value;
    }

    public void ExtendAppOpenTime()
    {
        AppOpenTimer = Time.time + NextAppOpenDelay;
    }

    public void OnAppStateChanged(AppState state)
    {
        if (EnableAppOpen && state == AppState.Foreground)
             ThreadDispatcher.Enqueue(ShowAppOpen);
    }

    public void ShowAppOpen()
    {
        if (!AdConstants.AdsRemoved && !IsForFamily)
        {
            if (Time.time > AppOpenTimer && Admob.IsReady)
            {
                ExtendAppOpenTime();
                Admob.ShowAppOpen();
            }
            //MobileToast.Show_DevMode($"AppOpen Left : {(AppOpenTimer - Time.time).ToString("F2")}s");
        }
    }

    #endregion

    #region Consent
    public static void PassAdjustConsentParameters()
    {
        var eurArea = ConsentManager.isEurArea ? "1" : "0";
        var userData = ConsentManager.isPersonalized ? "1" : "0";
        AdjustThirdPartySharing adjustThirdPartySharing = new AdjustThirdPartySharing(null);
        adjustThirdPartySharing.addGranularOption("google_dma", "eea", eurArea);
        adjustThirdPartySharing.addGranularOption("google_dma", "ad_personalization", userData);
        adjustThirdPartySharing.addGranularOption("google_dma", "ad_user_data", userData);
        Adjust.trackThirdPartySharing(adjustThirdPartySharing);
    }
    #endregion

    #region Misc

    public void VisitPrivacyPolicy()
    {
        ExtendAppOpenTime();

        if (string.IsNullOrEmpty(PrivacyPolicy)) MobileToast.Show("PrivacyPolicy is missing!", false);
        else Application.OpenURL(PrivacyPolicy);
    }

    float RamSize = -1;
    /// <summary> ///  Returns system memory in GB /// </summary>
    public float Ram()
    {
        if (RamSize.Equals(-1))
            RamSize = Mathf.CeilToInt(((float)SystemInfo.systemMemorySize / 102.4f)) / 10f;

        return RamSize;
    }

    #endregion

    #region GAID
    public static string GetAdvertisingID()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass advertisingIdClass = new AndroidJavaClass("com.kokozone.device.KZMonetization");
            return advertisingIdClass.CallStatic<string>("getAdvertisingId", GetAndroidContext());
        }
        catch (System.Exception)
        {

        }
#endif

        return null;
    }

    static AndroidJavaObject GetAndroidContext()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }
    #endregion
}

#region Enums
public enum BannerWidth { Full, Half }
#endregion