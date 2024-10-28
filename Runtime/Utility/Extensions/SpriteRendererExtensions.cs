using UnityEngine;

namespace TapEmpire.Utility
{
    public static class SpriteRendererExtensions
    {
        public static void AdjustRectSize(this SpriteRenderer self, Vector2 sourceSize, float leftRightOffset, float topBottomOffset)
        {
            var adjustedWidth = sourceSize.x - (2 * leftRightOffset);
            var adjustedHeight = sourceSize.y - (2 * topBottomOffset);
            var mainSectionSize = new Vector2(adjustedWidth, adjustedHeight);
            self.size = mainSectionSize;
        }

        public static void SetAlpha(this SpriteRenderer self, float alpha)
        {
            var color = self.color;
            color.a = alpha;
            self.color = color;
        }

        // for circles
        public static float GetRadius(this SpriteRenderer self)
        {
            var size = self.sprite.bounds.size;
            var smallestDimension = Mathf.Min(size.x, size.y);
            var spriteRendererTransform = self.transform;
            var lossyScale = spriteRendererTransform.lossyScale;
            var scale = (lossyScale.x + lossyScale.y) * 0.5f;
            return smallestDimension * 0.5f * scale;
        }

        public static Vector3[] GetWorldCorners(this SpriteRenderer self)
        {
            var corners = new Vector3[4];
            var bounds = self.bounds;

            corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            corners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);

            return corners;
        }

        public static void AdjustToScreenSize(this SpriteRenderer self, Vector2 screenSize, Camera camera)
        {
            var bottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            var topRight = camera.ScreenToWorldPoint(new Vector3(screenSize.x, screenSize.y, camera.nearClipPlane));
            var worldScreenWidth = topRight.x - bottomLeft.x;
            var worldScreenHeight = topRight.y - bottomLeft.y;
            var boundsSize = self.sprite.bounds.size;
            var newSize = new Vector2(worldScreenWidth / boundsSize.x, worldScreenHeight / boundsSize.y);
            self.transform.localScale = new Vector2(newSize.x, newSize.y);
        }

        public static Vector2 GetSize(this SpriteRenderer self)
        {
            return self.sprite.bounds.size;
        }

        public static bool IsFullyInView(this SpriteRenderer self, Camera camera)
        {
            Bounds spriteBounds = self.bounds;
            Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

            foreach (Plane plane in cameraPlanes)
            {
                if (plane.GetSide(spriteBounds.min) && plane.GetSide(spriteBounds.max))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static void CopyTo(this SpriteRenderer self, GameObject target)
        {
            SpriteRenderer targetRenderer = target.GetComponent<SpriteRenderer>();
            if (targetRenderer == null)
            {
                targetRenderer = target.AddComponent<SpriteRenderer>();
            }

            targetRenderer.sprite = self.sprite;
            targetRenderer.color = self.color;
            targetRenderer.flipX = self.flipX;
            targetRenderer.flipY = self.flipY;
            targetRenderer.sharedMaterial = self.sharedMaterial;
            targetRenderer.sortingLayerID = self.sortingLayerID;
            targetRenderer.sortingOrder = self.sortingOrder;
            targetRenderer.drawMode = self.drawMode;
            targetRenderer.size = self.size;
            targetRenderer.tileMode = self.tileMode;
            targetRenderer.maskInteraction = self.maskInteraction;
            targetRenderer.enabled = self.enabled;
        }
    }
}