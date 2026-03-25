#if TEL_CLOUD_SAVE
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services
{
    public class CloudSaveRestoreUIViewBase : UIView<CloudSaveRestoreUIViewModel>, IInjectable
    {
        [SerializeField] private Button _acceptButton = null;
        [SerializeField] private Button _declineButton = null;
        [SerializeField] private Button _cancelButton = null;
        
        protected readonly CompositeDisposable Disposables = new();
        
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _acceptButton.onClick.Subscribe(DerivedModel.OnAcceptPressed).AddTo(Disposables);
            _declineButton.onClick.Subscribe(DerivedModel.OnDeclinePressed).AddTo(Disposables);
            _cancelButton.onClick.Subscribe(DerivedModel.OnDeclinePressed).AddTo(Disposables);

            OnFinallyOpenAsync();
            
            return base.OnOpenAsync(cancellationToken);
        }

        protected virtual void OnFinallyOpenAsync()
        {
            
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            Disposables.Dispose();
            return base.OnCloseAsync(cancellationToken);
        }
    }
}
#endif