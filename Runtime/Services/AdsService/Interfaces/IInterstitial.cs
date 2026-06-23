using R3;

namespace TapEmpire.Services
{
    public interface IInterstitial : IAd
    {
        ReactiveProperty<bool> IsLoaded { get; }
        ReactiveProperty<bool> IsShowing { get; }
        Subject<Unit> OnReward { get; }

        bool HasInterstitial(bool doRequest = false);

        void Show(string placement);
    }
}
