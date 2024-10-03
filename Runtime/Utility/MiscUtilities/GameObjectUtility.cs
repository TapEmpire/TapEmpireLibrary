using RagDoll.Utility;
using UnityEngine;

namespace TapEmpire.Utility
{
    public static class GameObjectsUtility
    {
        public static GameObject CreateSiblingDuplicate(GameObject origin)
        {
            GameObject duplicate = GameObject.Instantiate(origin, origin.transform.parent);
            int originalIndex = origin.transform.GetSiblingIndex();
            duplicate.transform.SetSiblingIndex(originalIndex + 1);
            return duplicate;
        }

        public static GameObject CreateSiblingGameObject(GameObject origin, string name)
        {
            GameObject sibling = new GameObject(name);
            int originalIndex = origin.transform.GetSiblingIndex();
            sibling.transform.parent = origin.transform.parent;
            sibling.transform.SetSiblingIndex(originalIndex + 1);
            sibling.transform.CopyFromOtherTransform(origin.transform);
            return sibling;
        }
    }
}