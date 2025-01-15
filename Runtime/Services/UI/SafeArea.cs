using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.UI
{
    public class SafeArea : MonoBehaviour
    {
        RectTransform rectTransform;
        Rect safeArea;
        Vector2 minAnchor;
        Vector2 maxAnchor;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            RefreshSafeAreaAnchors();
        }

        void RefreshSafeAreaAnchors()
        {
            safeArea = Screen.safeArea;

            minAnchor = safeArea.position;
            maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}
