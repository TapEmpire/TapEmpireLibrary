using R3;
using TapEmpire.UI;

namespace TapEmpire.Services
{
    public class RateMeUiViewModel : IUIViewModel
    {
        public ReactiveCommand<Unit> AcceptCommand { get; } = new();
        public ReactiveCommand<Unit> RejectCommand { get; } = new();
    }
}