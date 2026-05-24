using R3;
using UnityEngine;

namespace TapEmpire.Experimental
{
    public interface IBanner : IAd
    {
        ReactiveProperty<bool> IsLoaded { get; }
        ReactiveProperty<Vector2> Layout { get; }

        void Show();
        void Hide();
    }
}
