using UnityEngine;

namespace TapEmpire.Utility
{
    public static class ComponentExtensions
    {
        public static void DestroyAllChildrenOfType<T>(this Component self, bool includeInActive = true) where T : Component
        {
            var components = self.GetComponentsInChildren<T>(includeInActive);
            foreach (var component in components)
            {
                ObjectUtility.Destroy(component.gameObject);
            }
        }


        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            if (component == null)
            {
                component = self.AddComponent<T>();
            }
            return component;
        }
    }
}