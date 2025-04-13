using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace TapEmpire.Utility
{
    public static partial class FileUtility
    {
        public static string OpenFilePanel(string title, string directory, string extension = "json")
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
#else
            return string.Empty;
#endif
        }

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

        public static void WriteFile(string filename, string text)
        {
#if UNITY_EDITOR
            if (filename.Length != 0)
            {
                File.WriteAllText(filename, text);
            }
#endif
        }

        public static string ReadText(string title, string directory)
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel(title, directory, "json");
            return ReadFile(path);
#else
            return string.Empty;
#endif
        }

        public static string ReadFile(string filename)
        {
#if UNITY_EDITOR
            return filename.Length != 0 ? File.ReadAllText(filename) : string.Empty;
#else
            return string.Empty;
#endif
        }
    }
}