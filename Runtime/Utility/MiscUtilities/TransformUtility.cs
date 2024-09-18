using System;
using System.Collections.Generic;
using TapEmpire.Utility;
using UnityEngine;

namespace RagDoll.Utility
{
    public static class TransformUtility
    {
        public static bool TryFindInChildrenByNameRecursive(this Transform self, string name, out Transform result)
        {
            foreach (Transform child in self)
            {
                if (child.name == name)
                {
                    result = child;
                    return true;
                }
                if (child.TryFindInChildrenByNameRecursive(name, out result))
                {
                    return true;
                }
            }
            result = null;
            return false;
        }
        
        public static bool TryFindAllInChildrenRecursive(this Transform self, Func<Transform, bool> findDelegate, out List<Transform> results)
        {
            results = new List<Transform>();
            FindAllInChildrenRecursive(self, findDelegate, results);
            return results.Count > 0;
        }

        
        private static void FindAllInChildrenRecursive(this Transform self, Func<Transform, bool> findDelegate, List<Transform> results)
        {
            foreach (Transform child in self)
            {
                if (findDelegate.Invoke(child))
                {
                    results.Add(child);
                }
                FindAllInChildrenRecursive(child, findDelegate, results);
            }
        }
        
        public static bool TryFindAllInChildren<T>(this Transform self, out T[] results) where T : Component
        {
            using (ListScope<T>.Create(out var list))
            {
                foreach (Transform child in self)
                {
                    if (child.TryGetComponent<T>(out var result))
                    {
                        list.Add(result);
                    }
                }
                if (list.Count > 0)
                {
                    results = list.ToArray();
                    return true;
                }
                results = null;
                return false;
            }
        }

        public static void CopyFromOtherTransform(this Transform self, Transform other)
        {
            self.position = other.position;
            self.rotation = other.rotation;
            self.localScale = other.localScale;
        }

        public static void RotateTowardsInAxises(this Transform self, Transform other, bool rotateX, bool rotateY, bool rotateZ)
        {
            var direction = other.position - self.position;
            var targetRotation = Quaternion.LookRotation(direction);

            var targetEulerAngles = targetRotation.eulerAngles;
            var currentEulerAngles = self.eulerAngles;

            if (rotateX) currentEulerAngles.x = targetEulerAngles.x;
            if (rotateY) currentEulerAngles.y = targetEulerAngles.y;
            if (rotateZ) currentEulerAngles.z = targetEulerAngles.z;

            self.rotation = Quaternion.Euler(currentEulerAngles);
        }
    }
}