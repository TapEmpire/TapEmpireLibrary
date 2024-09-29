#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class PrefabWithComponentDrawer : OdinAttributeDrawer<PrefabWithComponentAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var attribute = this.Attribute;
        var valueEntry = this.Property.ValueEntry;

        SirenixEditorGUI.BeginInlineBox();
        SirenixEditorGUI.BeginBoxHeader();
        {
            SirenixEditorGUI.Title($"{attribute.RequiredComponentType.Name} Prefab Selector", null, TextAlignment.Left, true);
        }
        SirenixEditorGUI.EndBoxHeader();

        GameObject selectedPrefab = valueEntry.WeakSmartValue as GameObject;

        if (SirenixEditorGUI.ObjectField(label, selectedPrefab, attribute.RequiredComponentType, false) is var newValue)
        {
            if (newValue != null)
            {
                valueEntry.WeakSmartValue = newValue;
            }
            else if (newValue != null)
            {
                Debug.LogWarning($"Selected object does not contain a {attribute.RequiredComponentType.Name} component.");
            }
            else
            {
                valueEntry.WeakSmartValue = null;
            }
        }

        SirenixEditorGUI.EndInlineBox();
    }
}
#endif