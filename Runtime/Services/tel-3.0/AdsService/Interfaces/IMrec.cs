using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IMrec : IDisposable
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<AdImpressionData> OnImpression { get; }

        void Show();
        void Show(int x, int y);
        void Hide();
    }
}
