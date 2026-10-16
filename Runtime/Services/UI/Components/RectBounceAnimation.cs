using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TapEmpire.UI
{
    public class RectBounceAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _bounceScale = 1.1f;
        [SerializeField] private float _pressDuration = 0.1f;
        [SerializeField] private float _bounceDuration = 0.15f;
        [SerializeField] private float _returnDuration = 0.1f;

        private readonly Subject<Unit> _onTapped = new();
        public Observable<Unit> OnTapped => _onTapped;

        private RectTransform _rect;
        private Vector3 _originalScale;
        private Vector2 _originalPivot;
        private bool _isPressed;
        private Sequence _sequence;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _originalScale = transform.localScale;
            _originalPivot = _rect.pivot;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _sequence?.Kill();
            SetPivot(new Vector2(0.5f, 0.5f));
            _sequence = DOTween.Sequence()
                .Append(transform.DOScale(_originalScale * _pressedScale, _pressDuration).SetEase(Ease.OutQuad))
                .SetLink(gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;
            _onTapped.OnNext(Unit.Default);
            PlayBounceRelease();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;
            PlayBounceRelease();
        }

        private void PlayBounceRelease()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .Append(transform.DOScale(_originalScale * _bounceScale, _bounceDuration).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(_originalScale, _returnDuration).SetEase(Ease.OutBack))
                .OnComplete(RestorePivot)
                .SetLink(gameObject);
        }

        private void SetPivot(Vector2 newPivot)
        {
            var delta = newPivot - _rect.pivot;
            var size = _rect.rect.size;
            _rect.pivot = newPivot;
            _rect.anchoredPosition += new Vector2(delta.x * size.x, delta.y * size.y);
        }

        private void RestorePivot()
        {
            SetPivot(_originalPivot);
        }
    }
}
