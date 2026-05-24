using R3;

namespace TapEmpire.Experimental
{
    public interface IRewarded : IAd
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<Unit> OnReward { get; }

        bool HasRewarded(bool doRequest = false);

        void Show(string placement);
    }
}
