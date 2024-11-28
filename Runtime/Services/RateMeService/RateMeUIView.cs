using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services
{
    public class RateMeUIView : UIView<RateMeUiViewModel>
    {
        [SerializeField] private Button _accept;
        [SerializeField] private Button _reject;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _accept.onClick.AddListener(Accept);
            _reject.onClick.AddListener(Reject);
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
            _accept.onClick.RemoveAllListeners();
            _reject.onClick.RemoveAllListeners();
            return UniTask.CompletedTask;
        }
    }
}