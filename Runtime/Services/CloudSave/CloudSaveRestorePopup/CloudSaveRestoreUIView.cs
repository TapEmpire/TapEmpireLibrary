using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Services
{
    public class CloudSaveRestoreUIView : UIView<CloudSaveRestoreUIViewModel>, IInjectable
    {
        [SerializeField] private Button _acceptButton = null;
        [SerializeField] private Button _declineButton = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private TMP_Text _dateText = null;

        private readonly CompositeDisposable _disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _acceptButton.onClick.Subscribe(DerivedModel.OnAcceptPressed).AddTo(_disposables);
            _declineButton.onClick.Subscribe(DerivedModel.OnDeclinePressed).AddTo(_disposables);
            _cancelButton.onClick.Subscribe(DerivedModel.OnDeclinePressed).AddTo(_disposables);
            if (_dateText)
            {
                _dateText.text = DerivedModel.CloudDataDate.ToString("g");
            }
            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return base.OnCloseAsync(cancellationToken);
        }
    }
}
