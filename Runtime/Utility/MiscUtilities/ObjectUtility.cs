using System.Text;
using UnityEditor;
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

        // public static void PrintAllProperties(SerializedObject obj, StringBuilder sb, string indent = "")
        // {
        //     SerializedProperty property = obj.GetIterator();
        //     bool enterChildren = true;

        //     while (property.NextVisible(enterChildren))
        //     {
        //         enterChildren = true;

        //         sb.AppendLine($"{indent}Property: {property.propertyPath}, Type: {property.propertyType}, Value: {GetPropertyValue(property)}");

        //         if (property.hasVisibleChildren && property.propertyType != SerializedPropertyType.String)
        //         {
        //             if (property.isArray)
        //             {
        //                 sb.AppendLine($"{indent}  Array Size: {property.arraySize}");
        //                 for (int i = 0; i < property.arraySize; i++)
        //                 {
        //                     var element = property.GetArrayElementAtIndex(i);
        //                     sb.AppendLine($"{indent}  Element {i}: {GetPropertyValue(element)}");
        //                 }
        //             }

        //             enterChildren = false;
        //         }
        //     }
        // }

        // private static string GetPropertyValue(SerializedProperty property)
        // {
        //     switch (property.propertyType)
        //     {
        //         case SerializedPropertyType.Integer:
        //             return property.intValue.ToString();
        //         case SerializedPropertyType.Boolean:
        //             return property.boolValue.ToString();
        //         case SerializedPropertyType.Float:
        //             return property.floatValue.ToString();
        //         case SerializedPropertyType.String:
        //             return property.stringValue;
        //         case SerializedPropertyType.Enum:
        //             return property.enumValueIndex.ToString();
        //         case SerializedPropertyType.ObjectReference:
        //             return property.objectReferenceValue != null ? property.objectReferenceValue.name : "null";
        //         default:
        //             return "(Complex type)";
        //     }
        // }
    }
}