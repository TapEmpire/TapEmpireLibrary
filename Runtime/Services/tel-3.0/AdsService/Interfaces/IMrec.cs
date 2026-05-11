using R3;

namespace TapEmpire.Experimental
{
    public interface IMrec
    {
        ReadOnlyReactiveProperty<bool> IsLoaded { get; }

        Observable<AdImpressionData> OnImpression { get; }
        Observable<AdError> OnFailed { get; }

        void Show();
        void Show(int x, int y);
        void Hide();
        void Destroy();
    }
}
