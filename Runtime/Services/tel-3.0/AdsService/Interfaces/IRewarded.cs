using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Experimental
{
    public interface IRewarded
    {
        ReadOnlyReactiveProperty<bool> IsLoaded { get; }

        Observable<AdImpressionData> OnImpression { get; }
        Observable<AdError> OnFailed { get; }

        UniTask<AdRewardEvent?> ShowAsync(string placement, CancellationToken cancellationToken = default);
    }
}
