using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.UI
{
    public static class ShibariExtensions
    {
        // The depth defaults to the camera's own distance to the zero plane, where a 2D game keeps
        // everything it renders.
        public static Vector3 GetWorldPoint(this IUIService uiService, string name, Camera camera)
        {
            return uiService.GetWorldPoint(name, camera, -camera.transform.position.z);
        }

        public static Vector3 GetWorldPoint(this IUIService uiService, string name, Camera camera, float depth)
        {
            var anchor = uiService.ShibariContext.TryGetValue(name);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, anchor.position);

            return camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
        }
    }
}
