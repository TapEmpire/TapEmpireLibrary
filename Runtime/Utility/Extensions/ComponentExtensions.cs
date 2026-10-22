using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TapEmpire.Utility
{
    public static class ComponentExtensions
    {
        public static Vector3 Center(this IReadOnlyCollection<Component> self)
            => self.Aggregate(Vector3.zero, (sum, component) => sum + component.transform.position) / self.Count;

        public static T[] AsArray<T>(this T self) where T : Component
            => (Component)self != null ? new[] { self } : Array.Empty<T>();

        public static IDisposable ShiftSorting(this IReadOnlyCollection<Component> self, int offset)
        {
            var renderers = self
                .Where(component => component != null)
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

        public static IDisposable FocusSorting(this Component self, int order, bool raycastable = false)
        {
            if (self == null || self.transform is not RectTransform) return Disposable.Empty;

            return self.TryGetComponent<Canvas>(out var canvas)
                ? FocusCanvas(canvas, order)
                : FocusWithCanvas(self.gameObject, order, raycastable);
        }

        private static IDisposable FocusCanvas(Canvas canvas, int order)
        {
            var wasOverriding = canvas.overrideSorting;
            var previousOrder = canvas.sortingOrder;

            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            return Disposable.Create(() =>
            {
                if (canvas == null) return;

                canvas.overrideSorting = wasOverriding;
                canvas.sortingOrder = previousOrder;
            });
        }

        private static IDisposable FocusWithCanvas(GameObject target, int order, bool raycastable)
        {
            var canvas = target.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            var raycaster = raycastable ? target.AddComponent<GraphicRaycaster>() : null;

            return Disposable.Create(() =>
            {
                if (raycaster != null) Object.Destroy(raycaster);
                if (canvas != null) Object.Destroy(canvas);
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