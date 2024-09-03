namespace TapEmpire.Services
{
    public interface IAdsService : IService
    {
        System.Action<string> OnAdReceivedRewardEvent { get; set; }
        System.Action<string> OnAdReceivedOnceRewardEvent { get; set; }
        System.Action<string> OnAdDisplayedRewardEvent { get; set; }
        System.Action<string> OnAdClickedEvent { get; set; }
        System.Action<bool> OnInterstitialAdShowRequested { get; set; }

        System.Action OnRewardedAdReady { get; set; }

        bool IsRewardedAdReady { get; }
        bool IsInterstitialReady { get; }

        void ShowInterstitial();
        void ShowInterstitial(int level, System.Action action);
        void ShowRewarded(string adType);
        void DisableAds(bool shouldDisable);
        void ShowInterstitialByTimer();

        bool AdsDisabled { get; }
    }
}
