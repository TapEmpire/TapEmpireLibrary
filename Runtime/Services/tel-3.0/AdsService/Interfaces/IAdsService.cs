using R3;

namespace TapEmpire.Experimental
{
    public interface IAdsService
    {
        IBanner Banner { get; }
        IInterstitial Interstitial { get; }
        IRewarded Rewarded { get; }
        IMrec Mrec { get; }

        ReadOnlyReactiveProperty<bool> AdsEnabled { get; }
        Observable<Unit> OnInitialized { get; }

        void DisableAds(bool shouldDisable);
    }
}
