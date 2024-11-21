using R3;
using TapEmpire.UI;

namespace RagDoll.UI
{
    public class RateUsUiViewModel : IUIViewModel
    {
        public ReactiveCommand<Unit> FunGameCommand { get; } = new();
        public ReactiveCommand<Unit> BoringCommand { get; } = new();
    }
}