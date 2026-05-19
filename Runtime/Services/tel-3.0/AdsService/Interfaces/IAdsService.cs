using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IAdsService
    {
        Observable<Unit> OnInitialized { get; }

        ReadOnlyReactiveProperty<bool> AdsEnabled { get; }
        ReadOnlyReactiveProperty<bool> IsInterstitialReady { get; }
        ReadOnlyReactiveProperty<bool> IsRewardedReady { get; }

        void ShowBanner();
        void HideBanner();
        void ShowInterstitial(string placement);
        void ShowRewarded(string placement, Action onReward);
        void ShowMrec();
        void ShowMrec(int x, int y);
        void HideMrec();

        void DisableAds(bool shouldDisable);
    }
}
