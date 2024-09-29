using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public interface IUILocker
    {
        public void InitCanvas(RectTransform canvasRect);
        public void UnmaskSpriteRendererInView(Camera mainCamera, RectTransform viewParent,
            params SpriteRenderer[] targetRenderers);
        public void Disable(bool withFadeOut = true);
        public void LockExceptSpriteRenderer(Camera mainCamera, SpriteRenderer spriteRenderer,
            RectTransform viewParent, System.Action clickCallback, Sprite substituteSprite = null);
        public void LockExceptImage(Image targetImage, RectTransform viewParent, System.Action clickCallback);
    }
}