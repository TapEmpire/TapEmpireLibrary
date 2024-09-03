using UnityEngine;

namespace TapEmpire.Utility
{
    public static class TransformExtensions
    {
        public static bool TryGetChildWithName(this Transform self, string childName, out Transform child)
        {
            for (var index = 0; index < self.childCount; index++)
            {
                var currentChild = self.GetChild(index);
                if (currentChild.gameObject.name != childName)
                {
                    continue;
                }
                child = currentChild;
                return true;
            }
            child = null;
            return false;
        }

        public static void CompensateScale(this Transform self, Transform other)
        {
            self.localScale = self.localScale.Divide(other.localScale);
        }

        public static Transform GetOrCreateChildWithName(this Transform self, string childName)
        {
            if (self.TryGetChildWithName(childName, out var child))
            {
                return child;
            }
            child = new GameObject(childName).transform;
            child.parent = self;
            return child;
        }

        public static void SetZ(this Transform self, float z)
        {
            var position = self.position;
            position.z = z;
            self.position = position;
        }

        public static void SetXY(this Transform self, Vector2 position)
        {
            self.position = new Vector3(position.x, position.y, self.position.z);
        }
        
        public static void SetXYLocal(this Transform self, Vector2 position)
        {
            self.position = new Vector3(position.x, position.y, self.position.z);
        }
        
        public static void DestroyAllChildren(this Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
            {
                Object.Destroy(self.GetChild(i).gameObject);
            }
        }
    }
}