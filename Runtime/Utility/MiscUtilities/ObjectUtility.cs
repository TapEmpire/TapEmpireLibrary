using UnityEngine;

namespace TapEmpire.Utility
{
    public static class ObjectUtility
    {
        public static void Destroy(Object unityObject)
        {
            if (Application.isEditor)
            {
                Object.DestroyImmediate(unityObject);
            }
            else
            {
                Object.Destroy(unityObject);
            }
        }
    }
}