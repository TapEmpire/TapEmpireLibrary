using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IAdsService
    {
        Subject<Unit> OnInitialized { get; }
        Subject<Unit> OnReceivedReward { get; }
        Subject<string> OnAdClicked { get; }

        ReadOnlyReactiveProperty<bool> AdsEnabled { get; }
        ReadOnlyReactiveProperty<bool> IsInterstitialReady { get; }
        ReadOnlyReactiveProperty<bool> IsRewardedReady { get; }
        bool CanShowRewarded { get; }

        bool SkipAds { get; set; }

        void ShowBanner();
        void HideBanner();
        void ShowInterstitial(string placement);
        void ShowRewarded(string placement, Action onRewardCallback);
        void ShowMrec();
        void ShowMrec(int x, int y);
        void HideMrec();

        void DisableAds(bool shouldDisable);
    }
}
