using R3;

namespace TapEmpire.Experimental
{
    public interface IInterstitial
    {
        Observable<AdImpressionData> OnImpression { get; }
        Observable<Unit> OnReward { get; }

        bool HasInterstitial(bool doRequest = false);

        void Show(string placement);
    }
}
