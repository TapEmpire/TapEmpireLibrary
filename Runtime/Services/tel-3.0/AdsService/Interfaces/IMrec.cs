using R3;

namespace TapEmpire.Experimental
{
    public interface IMrec
    {
        ReactiveProperty<bool> IsLoaded { get; }
        Subject<AdImpressionData> OnImpression { get; }

        void Show();
        void Show(int x, int y);
        void Hide();
        void Destroy();
    }
}
