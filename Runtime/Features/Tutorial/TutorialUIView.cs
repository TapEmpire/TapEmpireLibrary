using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Feature.Tutorial
{
    public abstract class TutorialUIView : UIView<TutorialUIViewModel>, IInjectable
    {
        [SerializeField] private Button _nextButton = null;
        [SerializeField] private Button _closeButton = null;

        protected readonly CompositeDisposable Disposables = new();

        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _nextButton.onClick.Subscribe(OnNextPressed).AddTo(Disposables);

            if (_closeButton)
            {
                _closeButton.gameObject.SetActive(DerivedModel.IsSkippable);
                _closeButton.onClick.Subscribe(DerivedModel.OnNextPressed).AddTo(Disposables);
            }

            return base.OnOpenAsync(cancellationToken);
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            Disposables.Dispose();
            return base.OnCloseAsync(cancellationToken);
        }

        protected abstract void OnNextPressed();
    }
}
