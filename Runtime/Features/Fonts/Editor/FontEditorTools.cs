using System.Collections.Generic;
using TMPro;
using UnityEditor;
using TapEmpire.Utility;

namespace TapEmpire.Fonts
{
    public static class FontEditorTools
    {
        [MenuItem("TapEmpire/Tools/Adjust Fonts #&f")]
        private static void AdjustFonts()
        {
            var fontSettings = EditorCustomUtility.LoadFirstAsset<FontSettings>();

            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set Character Spacing");

            foreach (var go in Selection.gameObjects)
            {
                foreach (var text in go.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (text.TryGetComponent<FontToolsIgnorer>(out var ignore)) continue;

                    var fontData = fontSettings.FontSpacings.TryGetValue(text.fontSharedMaterial);

                    Undo.RecordObject(text, "Set Character Spacing");
                    text.characterSpacing = fontData.CharacterSpacing;
                    text.wordSpacing = fontData.WordSpacing;
                    EditorUtility.SetDirty(text);
                }
            }

            Undo.CollapseUndoOperations(group);
        }
    }
}
