using UnityEngine;

namespace TapEmpire.Utility
{
    public static class RectTransformExtensions
    {
        public static void CoverTargetRenderer(this RectTransform self, SpriteRenderer targetRenderer, Camera camera, RectTransform canvasRect)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            //Vector2 referenceResolution = canvasScaler.referenceResolution;
            //Vector2 refRatio = new Vector2(referenceResolution.x / screenSize.x, referenceResolution.y / screenSize.y);
            //float scaleFactor = Mathf.Min(refRatio.x, refRatio.y);

            var bounds = targetRenderer.bounds;
            var worldCorners = new Vector3[4];
            worldCorners[0] = bounds.min;
            worldCorners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            worldCorners[2] = bounds.max;
            worldCorners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

            Vector2 screenMin = camera.WorldToScreenPoint(worldCorners[0]);
            Vector2 screenMax = camera.WorldToScreenPoint(worldCorners[0]);

            for (var i = 1; i < worldCorners.Length; i++)
            {
                var screenPoint = camera.WorldToScreenPoint(worldCorners[i]);
                screenMin = Vector2.Min(screenMin, screenPoint);
                screenMax = Vector2.Max(screenMax, screenPoint);
            }
            screenMin.x /= screenSize.x;
            screenMin.y /= screenSize.y;
            screenMax.x /= screenSize.x;
            screenMax.y /= screenSize.y;
            var sizeDelta = canvasRect.sizeDelta;
            screenMin.x *= sizeDelta.x;
            screenMin.y *= sizeDelta.y;
            screenMax.x *= sizeDelta.x;
            screenMax.y *= sizeDelta.y;

            self.anchoredPosition = (screenMin + screenMax) * 0.5f - (sizeDelta * 0.5f);
            self.sizeDelta = (screenMax - screenMin);// / scaleFactor;
        }
        
        public static void PositionBetweenUIAndWorld(this RectTransform target, RectTransform topRect, SpriteRenderer bottomSprite, RectTransform root, Camera camera)
        {
            var topY = topRect.position.y;
            
            var bounds = bottomSprite.bounds;
            var spriteTopPoint = new Vector2(bounds.center.x, bounds.max.y);
            Vector3 spriteTopScreen = RectTransformUtility.WorldToScreenPoint(camera, spriteTopPoint);
            var bottomY = spriteTopScreen.y;
            
            var middleY = (topY - bottomY) / 2 + bottomY;
            var middleScreen = new Vector2(spriteTopScreen.x, middleY);

            var originalParent = target.parent;
            target.SetParent(root, true);

            target.anchorMin = target.anchorMax = target.pivot = new Vector2(0.5f, 0.5f);
            target.position = new Vector2(0, middleScreen.y);
            target.sizeDelta = new Vector2(root.rect.width, target.sizeDelta.y);

            target.SetParent(originalParent, true);
            target.localPosition = new Vector2(0, target.localPosition.y);
        }
    }
}