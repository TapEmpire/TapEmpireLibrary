using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class SceneLoadingUIView : UIView<SceneLoadingUIViewModel>
    {
        [SerializeField]
        private Image _fillImage;

        [SerializeField]
        private RectTransform _emptyTransform;

        [SerializeField]
        private RectTransform _filledTransform;

        [SerializeField]
        private Ease _ease = Ease.OutQuad;

        private float _currentProgress;
        
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            DerivedModel.SetProgressCallback += OnSetProgressCallback;
            _currentProgress = 0;
            _fillImage.rectTransform.sizeDelta = _emptyTransform.sizeDelta;
            _fillImage.rectTransform.position = _emptyTransform.position;
            return base.OnOpenAsync(cancellationToken);
        }
        
        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            DerivedModel.SetProgressCallback -= OnSetProgressCallback;
            return base.OnCloseAsync(cancellationToken);
        }

        private void OnSetProgressCallback(float progress, float duration)
        {
            var targetSizeDelta = Vector2.Lerp(_emptyTransform.sizeDelta, _filledTransform.sizeDelta, progress);
            var targetPosition = Vector3.Lerp(_emptyTransform.localPosition, _filledTransform.localPosition, progress);
            _fillImage.rectTransform.DOKill();
            _fillImage.rectTransform.DOSizeDelta(targetSizeDelta, duration).SetEase(_ease);
            _fillImage.rectTransform.DOLocalMove(targetPosition, duration).SetEase(_ease);
        }
    }
}