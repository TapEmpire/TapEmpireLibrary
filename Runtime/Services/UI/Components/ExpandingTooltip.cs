using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace TapEmpire.UI
{
    public class ExpandingTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private LocalizeStringEvent _textLocalization;
        [SerializeField] private float _collapsedWidth = 200f;
        [SerializeField] private float _showDuration = 0.4f;
        [SerializeField] private float _hideDuration = 0.2f;

        private Sequence _sequence;
        private Vector2 _size;

        private Vector2 Collapsed => new(_collapsedWidth, _size.y);

        private void Awake()
        {
            _size = _background.sizeDelta;

            _background.sizeDelta = Collapsed;
            _text.alpha = 0f;
        }

        public void SetText(LocalizedString text)
        {
            _textLocalization.StringReference = text;
        }

        public void Show()
        {
            _background.gameObject.SetActive(true);

            Play(_size, 1f, _showDuration, Ease.OutQuad, Ease.InQuad);
        }

        public void Hide()
        {
            if (!_background.gameObject.activeSelf) return;

            Play(Collapsed, 0f, _hideDuration, Ease.Linear, Ease.OutQuad)
                .OnComplete(() => _background.gameObject.SetActive(false));
        }

        private Sequence Play(Vector2 size, float alpha, float duration, Ease backgroundEase, Ease textEase)
        {
            _sequence?.Kill();

            _sequence = DOTween.Sequence()
                .Join(_background.DOSizeDelta(size, duration).SetEase(backgroundEase))
                .Join(_text.DOFade(alpha, duration).SetEase(textEase))
                .SetLink(gameObject);

            return _sequence;
        }
    }
}
