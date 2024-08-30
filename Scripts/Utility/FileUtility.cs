using System.IO;
using UnityEditor;

namespace TapEmpire.Utility
{
    public static class FileUtility
    {
#if UNITY_EDITOR
        public static void SaveText(string title, string fileName, string text)
        {
            var path = EditorUtility.SaveFilePanel(
                title,
                "",
                fileName + ".json",
                "json");

            if (path.Length != 0)
            {
                File.WriteAllText(path, text);
            }
        }
#endif
    }
}