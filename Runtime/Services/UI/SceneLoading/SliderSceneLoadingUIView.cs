using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class SliderSceneLoadingUIView : SceneLoadingUIView
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _progressBarFillSlider;

        private float _currentProgress;
        
        protected override UniTask OnOpenAsync(CancellationToken cancellationToken)
        {
            _progressBarFillSlider.value = 0;
            _canvasGroup.alpha = 1;
            return base.OnOpenAsync(cancellationToken);
        }
        
        protected override async UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            await base.OnCloseAsync(cancellationToken);
            await _canvasGroup.DOFade(0, 0.3f).WithCancellation(cancellationToken);
        }

        protected override void OnSetProgressCallback(float progress, float duration)
        {
            _progressBarFillSlider.DOKill();
            _progressBarFillSlider.DOValue(progress, duration);
        }
    }
}