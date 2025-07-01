using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services
{
    public class RateMeUIView : UIView<RateMeUiViewModel>, IInjectable
    {
        [SerializeField] private Button _accept;
        [SerializeField] private Button _reject;
        [SerializeField] private Button _close;

        private CompositeDisposable _disposables = new();

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _accept.onClick.Subscribe(Accept).AddTo(_disposables);
            _reject.onClick.Subscribe(Reject).AddTo(_disposables);
            _close?.onClick.Subscribe(Reject).AddTo(_disposables);
            return UniTask.CompletedTask;
        }

        private void Accept()
        {
            DerivedModel.AcceptCommand.Execute(Unit.Default);
        }

        private void Reject()
        {
            DerivedModel.RejectCommand.Execute(Unit.Default);
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return UniTask.CompletedTask;
        }
    }
}