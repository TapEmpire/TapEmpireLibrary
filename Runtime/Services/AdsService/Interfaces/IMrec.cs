using R3;

namespace TapEmpire.Services
{
    public interface IMrec : IAd
    {
        ReactiveProperty<bool> IsLoaded { get; }

        void Show();
        void Show(int x, int y);
        void Hide();
    }
}
