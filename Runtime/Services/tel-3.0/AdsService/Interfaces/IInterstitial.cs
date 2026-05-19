using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IInterstitial : IDisposable
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<AdImpressionData> OnImpression { get; }
        Subject<Unit> OnReward { get; }

        bool HasInterstitial(bool doRequest = false);

        void Show(string placement);
    }
}
