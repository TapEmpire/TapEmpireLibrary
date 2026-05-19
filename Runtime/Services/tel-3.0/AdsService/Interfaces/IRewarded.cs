using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IRewarded : IDisposable
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<AdImpressionData> OnImpression { get; }
        Subject<Unit> OnReward { get; }

        bool HasRewarded(bool doRequest = false);

        void Show(string placement);
    }
}
