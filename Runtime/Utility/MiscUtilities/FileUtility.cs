using System.IO;
using UnityEditor;

namespace TapEmpire.Utility
{
    public static class FileUtility
    {
        public static void SaveText(string title, string fileName, string text)
        {
#if UNITY_EDITOR
            var path = EditorUtility.SaveFilePanel(
                title,
                "",
                fileName + ".json",
                "json");

            if (path.Length != 0)
            {
                File.WriteAllText(path, text);
            }
#endif
        }

        public static string ReadText(string title, string directory)
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel(title, directory, "json");

            return path.Length != 0 ? File.ReadAllText(path) : string.Empty;
#else
            return string.Empty;
#endif
        }
    }
}