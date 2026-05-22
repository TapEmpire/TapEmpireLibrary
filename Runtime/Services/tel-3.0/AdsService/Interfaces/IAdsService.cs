using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IAdsService
    {
        Subject<Unit> OnInitialized { get; }
        Subject<Unit> OnReceivedReward { get; }
        Subject<string> OnAdClicked { get; }
        Subject<bool> OnInterstitialAttempt { get; }
        Subject<AdImpressionData> OnImpression { get; }

        AdsSettings Settings { get; }

        ReadOnlyReactiveProperty<bool> AdsEnabled { get; }
        ReadOnlyReactiveProperty<bool> IsInterstitialReady { get; }
        ReadOnlyReactiveProperty<bool> IsRewardedReady { get; }
        bool CanShowRewarded { get; }
        bool CanShowInterstitial { get; }

        bool SkipAds { get; set; }

        bool ShowBanner(bool shouldShow);
        void DisableBanner();
        void ShowInterstitial(string placement, Action onClose, bool skip = false);
        // level is 1-indexed
        void ShowInterstitial(int level, string placement, Action onClose);
        void ShowRewarded(string placement, Action onRewardCallback);
        void ShowMrec();
        void ShowMrec(int x, int y);
        void HideMrec();

        void DisableAds(bool shouldDisable);
    }
}
