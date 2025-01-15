using System.Collections;
using System.Collections.Generic;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.UI
{
    public class SafeAreaShift : MonoBehaviour
    {
        [SerializeField] private float yShift = -30.0f;

#if UNITY_IOS
        void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.position = MathUtility.yShift(rectTransform.position, yShift, true);
        }
#endif
    }
}
