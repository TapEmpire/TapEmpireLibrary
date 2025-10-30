using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace TapEmpire.Shortcuts
{
    public static class TapEmpireShortcuts
    {
        [MenuItem("TapEmpire/Shortcuts/Clear player prefs #&d")]
        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        
        [MenuItem("TapEmpire/Shortcuts/Recompile")]
        private static void Recompile()
        {
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}