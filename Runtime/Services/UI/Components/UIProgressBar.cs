using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class UIProgressBar : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] Image _fill;
        [SerializeField] float _duration = 0.25f;
        [SerializeField] Ease _ease = Ease.OutCubic;
        [SerializeField][Range(0.0f, 1.0f)] float _value = 0.0f;

        private float _emptyWidthPx => 0.0f;
        private float _fullWidthPx => _background.rectTransform.rect.width;
        private Tween _tween = null;

        public void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            float target = Mathf.Lerp(_emptyWidthPx, _fullWidthPx, value);

            _tween?.Kill();

            var rectTransform = _fill.rectTransform;
            rectTransform.sizeDelta = new Vector2(target, rectTransform.sizeDelta.y);
        }

        public void AnimateProgress(float value)
        {
            value = Mathf.Clamp01(value);
            float target = Mathf.Lerp(_emptyWidthPx, _fullWidthPx, value);

            var rectTransform = _fill.rectTransform;
            _tween?.Kill();
            _tween = rectTransform.DOSizeDelta(new Vector2(target, rectTransform.sizeDelta.y), _duration).SetEase(_ease);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            SetProgress(_value);
        }
#endif
    }
}