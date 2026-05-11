using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public interface IBanner
    {
        ReadOnlyReactiveProperty<bool> IsLoaded { get; }
        ReadOnlyReactiveProperty<Vector2> Layout { get; }

        Observable<AdImpressionData> OnImpression { get; }
        Observable<AdError> OnFailed { get; }

        void Show();
        void Hide();
        void Destroy();
    }
}
