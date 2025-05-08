using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TapEmpire.UI
{
    public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public float pressedScale = 0.9f;
        public float duration = 0.1f;

        private Vector3 originalScale;

        void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(originalScale * pressedScale, duration).SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        }
    }
}
