using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TEL.Utilities
{
    public static class EditorTools
    {
        public static void SetDirty(Object target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
#endif
        }

        public static void SetDirtyRecursive(GameObject target)
        {
            SetDirty(target);
            foreach (Transform child in target.transform)
            {
                SetDirtyRecursive(child.gameObject);
            }
        }

        public static void SetDirtyAll<T>(List<T> targets) where T : Object
        {
            targets.ForEach(target => SetDirty(target));
        }

        public static void SetProductName(string productName)
        {
#if UNITY_EDITOR
            PlayerSettings.productName = productName;
#endif
        }

        public static void SetAndroidPackageName(string packageName)
        {
#if UNITY_EDITOR
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
#endif
        }

#if UNITY_EDITOR
        private static GameObject CreateCanvasForJoysticOld(string uiPath, string tagToCompare)
        {
            var allTags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var allTagsProperty = allTags.FindProperty("tags");
            var tagFound = false;

            for (var i = 0; i < allTagsProperty.arraySize; i++)
            {
                SerializedProperty t = allTagsProperty.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagToCompare))
                {
                    tagFound = true;
                    break;
                }
            }

            if (!tagFound)
            {
                Debug.Log("New UI Tag Created");
                var index = allTagsProperty.arraySize;
                if (index == 0)
                {
                    index = 1;
                }
                allTagsProperty.InsertArrayElementAtIndex(index - 1);
                var serializedProperty = allTagsProperty.GetArrayElementAtIndex(index - 1);
                serializedProperty.stringValue = tagToCompare;
                allTags.ApplyModifiedProperties();
            }

            var uiCanvas = GameObject.FindGameObjectWithTag(tagToCompare);

            if (uiCanvas == null)
            {
                var parent = GameObject.Find("GUI");
                var eventresource = (GameObject)Resources.Load(uiPath + "EventSystem");

                if (eventresource != null)
                {
                    GameObject.Instantiate(eventresource, parent.transform);
                }
                else
                {
                    Debug.LogError("Event system not found at : " + uiPath + "EventSystem");
                    return null;
                }

                var canvas = (GameObject)Resources.Load(uiPath + "Canvas");

                if (canvas != null)
                {
                    uiCanvas = GameObject.Instantiate(canvas, parent.transform);
                }
                else
                {
                    Debug.LogError("Canvas not found at : " + uiPath + "Canvas");
                }
            }

            return uiCanvas;
        }
#endif
    }
}
