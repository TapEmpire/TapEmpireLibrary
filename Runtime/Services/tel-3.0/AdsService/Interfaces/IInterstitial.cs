using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Experimental
{
    public interface IInterstitial
    {
        ReadOnlyReactiveProperty<bool> IsLoaded { get; }

        Observable<AdImpressionData> OnImpression { get; }
        Observable<AdError> OnFailed { get; }
        Observable<Unit> OnClosed { get; }

        UniTask<bool> ShowAsync(string placement, CancellationToken cancellationToken = default);
    }
}
