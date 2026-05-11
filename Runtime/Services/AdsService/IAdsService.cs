using R3;

namespace TapEmpire.Services
{
    public interface IAdsService : IService
    {
        ReadOnlyReactiveProperty<bool> AdsEnabled { get; }
        Observable<Unit> OnAdsInitialized { get; }

        System.Action<string> OnAdReceivedRewardEvent { get; set; }
        System.Action<string> OnAdReceivedOnceRewardEvent { get; set; }
        System.Action<string> OnAdDisplayedRewardEvent { get; set; }
        System.Action<string> OnAdClickedEvent { get; set; }
        System.Action<bool> OnInterstitialAdShowRequested { get; set; }

        System.Action OnRewardedAdReady { get; set; }

        bool IsRewardedAdReady { get; }
        bool IsInterstitialReady { get; }

        bool ShowInterstitial(string placement = "");
        void ShowInterstitial(int level, System.Action action, string placement = "");
        void ShowRewarded(string adType);
        void DisableAds(bool shouldDisable);
        void ShowInterstitialByTimer();

        bool ShowInterstitial(System.Action action, string placement = "");
        void ShowRewarded(string placement, System.Action action);

        bool ShowBanners(bool shouldShow); // returns whether they were shown right now
        void EnableBanners(); // Enabled banner based on level
        void DisableBanners();
        bool ShowMrec(bool shouldShow);
        bool ShowMrec(bool shouldShow, int x, int y);
        bool AdsDisabled { get; }
        bool AdsDisabledDebug { get; set; }
        bool IsMeticaEnabled { get; }

        AdsSettings Settings { get; }
        void SetBannerSettings(BannerWidth bannerSize, GoogleMobileAds.Api.AdPosition bannerPos);

        bool CanShowRewarded(int levelIndex);
        bool CanShowRewarded();
    }
}
