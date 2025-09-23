using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;

namespace TapEmpire.Services
{
    public class IapLoadingView : UIView<IapLoadingViewModel>, IInjectable
    {
        private CompositeDisposable _disposables = new();

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return UniTask.CompletedTask;
        }
    }
}