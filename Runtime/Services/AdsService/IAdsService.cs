using R3;

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

        bool ShowInterstitial();
        void ShowInterstitial(int level, System.Action action);
        void ShowRewarded(string adType);
        void DisableAds(bool shouldDisable);
        void ShowInterstitialByTimer();
        void ShowAppOpen(System.Action action);

        bool AdsDisabled { get; }
        float MaxWaitingTime { get; }
        ReadOnlyReactiveProperty<bool> ShouldWaitAppOpen { get; }
    }
}
