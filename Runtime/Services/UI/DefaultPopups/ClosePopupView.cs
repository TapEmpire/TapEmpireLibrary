using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class ClosePopupView : UIView<ClosePopupViewModel>, IInjectable
    {
        [SerializeField] private Button _closeButton = null;

        private CompositeDisposable _disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _closeButton.onClick.Subscribe(DerivedModel.OnClosePressed).AddTo(_disposables);
            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return base.OnCloseAsync(cancellationToken);
        }
    }
}
