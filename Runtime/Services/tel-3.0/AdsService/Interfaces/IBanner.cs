using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public interface IBanner
    {
        ReactiveProperty<bool> IsLoaded { get; }
        ReactiveProperty<Vector2> Layout { get; }
        Subject<AdImpressionData> OnImpression { get; }

        void Show();
        void Hide();
        void Destroy();
    }
}
