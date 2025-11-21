using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TapEmpire.Utility
{
    public static class RaycastUtility
    {
        public static bool IsPointerOverUI()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                var pointer = Input.touches[0];

                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = pointer.position
                };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                return results.Count > 0;
            }

            return false;
#endif
        }
    }
}