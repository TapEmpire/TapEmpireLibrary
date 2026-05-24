using R3;

namespace TapEmpire.Experimental
{
    public interface IInterstitial : IAd
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<Unit> OnReward { get; }

        bool HasInterstitial(bool doRequest = false);

        void Show(string placement);
    }
}
