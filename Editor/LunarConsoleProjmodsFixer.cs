//
//  LunarConsoleProjmodsFixer.cs
//
//  Fixes duplicate symbol issue by removing Free version entries from Lunar.projmods
//  when Full version is also present.
//

#if (UNITY_IOS || UNITY_IPHONE) && UNITY_6000
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Text.RegularExpressions;

namespace LunarConsoleEditorInternal
{
    class LunarConsoleProjmodsFixer : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private static readonly string[] FreeEntriesToRemove = new string[]
        {
            "Free\\\\Console\\\\Plugin\\\\LUConsolePluginImp.m",
            "Free\\\\Controllers\\\\Actions\\\\LUActionController.m",
            "Free\\\\Controllers\\\\Actions\\\\LUActionRegistry.m",
            "Free\\\\Controllers\\\\Variables\\\\LUCVar.m"
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            FixProjmodsFile();
        }

        [MenuItem("Tools/Lunar Console/Fix Projmods Duplicates")]
        public static void FixProjmodsFile()
        {
            var projmodsPath = Path.Combine(Application.dataPath, "LunarConsole/Editor/iOS/Lunar.projmods");

            if (!File.Exists(projmodsPath))
            {
                Debug.LogWarning("[LunarConsoleProjmodsFixer] Lunar.projmods not found at: " + projmodsPath);
                return;
            }

            var content = File.ReadAllText(projmodsPath);
            var originalContent = content;
            var modified = false;

            foreach (var entry in FreeEntriesToRemove)
            {
                var pattern = $"\\s*\"{Regex.Escape(entry)}\",?";
                if (Regex.IsMatch(content, pattern))
                {
                    content = Regex.Replace(content, pattern + "\\r?\\n?", "\n");
                    modified = true;
                }
            }

            if (modified)
            {
                content = Regex.Replace(content, ",\\s*\\n(\\s*\\])", "\n$1");

                File.WriteAllText(projmodsPath, content);
                Debug.Log("[LunarConsoleProjmodsFixer] Removed Free version entries from Lunar.projmods to fix duplicate symbols");
            }
        }
    }
}
#endif
