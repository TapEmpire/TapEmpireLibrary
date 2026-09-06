using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class ComponentExtensions
    {
        public static Vector3 Center(this IReadOnlyCollection<Component> self)
            => self.Aggregate(Vector3.zero, (sum, component) => sum + component.transform.position) / self.Count;

        public static IDisposable ShiftSorting(this IReadOnlyCollection<Component> self, int offset)
        {
            var renderers = self
                .SelectMany(component => component.GetComponentsInChildren<SpriteRenderer>())
                .ToArray();

            foreach (var renderer in renderers)
            {
                renderer.sortingOrder += offset;
            }

            return Disposable.Create(() =>
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != null) renderer.sortingOrder -= offset;
                }
            });
        }

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