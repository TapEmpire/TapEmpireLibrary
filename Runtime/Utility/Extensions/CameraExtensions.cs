using UnityEngine;

namespace TapEmpire.Utility
{
    public static class CameraExtensions
    {
        public static Vector3 GetTransformScreenPoint(this Camera camera, Transform target, float yOffset)
        {
            return camera.WorldToScreenPoint(new Vector3(target.position.x, target.position.y + yOffset, target.position.z));
        }

        public static Vector2 GetSize(this Camera camera)
        {
            if (camera.orthographic)
            {
                float width = camera.orthographicSize * 2 * camera.aspect;
                float height = camera.orthographicSize * 2;

                return new Vector2(width, height);
            }
            else
            {
                Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
                Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));

                float width = topRight.x - bottomLeft.x;
                float height = topRight.y - bottomLeft.y;

                return new Vector2(width, height);
            }
        }
    }
}