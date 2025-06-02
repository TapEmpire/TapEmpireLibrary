using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class ManualSceneLoadingUIView : SceneLoadingUIView
    {
        [SerializeField]
        private Image _fillImage;

        [SerializeField]
        private RectTransform _emptyTransform;

        [SerializeField]
        private RectTransform _filledTransform;

        [SerializeField]
        private Ease _ease = Ease.OutQuad;

        [SerializeField]
        private GameObject _loadingPart = null;

        private CompositeDisposable _disposables = new();
        
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _fillImage.rectTransform.sizeDelta = _emptyTransform.sizeDelta;
            _fillImage.rectTransform.position = _emptyTransform.position;
            DerivedModel.IsLoadingVisible.Subscribe(value => _loadingPart.SetActive(value)).AddTo(_disposables);
            return base.OnOpenAsync(cancellationToken);
        }
        
        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            _fillImage.rectTransform.DOKill();
            _disposables.Dispose();
            return base.OnCloseAsync(cancellationToken);
        }

        protected override void OnSetProgressCallback(float progress, float duration)
        {
            var currentProgress = Mathf.InverseLerp(_emptyTransform.sizeDelta.x, _filledTransform.sizeDelta.x,
                _fillImage.rectTransform.sizeDelta.x);

            if (currentProgress > progress)
            {
                return;
            }

            var targetSizeDelta = Vector2.Lerp(_emptyTransform.sizeDelta, _filledTransform.sizeDelta, progress);
            var targetPosition = Vector3.Lerp(_emptyTransform.localPosition, _filledTransform.localPosition, progress);
            _fillImage.rectTransform.DOKill();
            _fillImage.rectTransform.DOSizeDelta(targetSizeDelta, duration).SetEase(_ease);
            _fillImage.rectTransform.DOLocalMove(targetPosition, duration).SetEase(_ease);
        }
    }
}