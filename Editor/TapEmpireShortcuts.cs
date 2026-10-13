using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace TapEmpire.Shortcuts
{
    public static class TapEmpireShortcuts
    {
        private const string ScreenshotsFolder = "Screenshots";

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

        [MenuItem("TapEmpire/Shortcuts/Take screenshot #&c")]
        private static void TakeScreenshot()
        {
            var size = Handles.GetMainGameViewSize();
            var name = $"{Application.productName}_{size.x}x{size.y}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            var path = Path.Combine(ScreenshotsFolder, name);

            Directory.CreateDirectory(ScreenshotsFolder);
            ScreenCapture.CaptureScreenshot(path);

            // The capture is taken on the next Game view render, which a paused or stopped editor does not do by itself.
            InternalEditorUtility.RepaintAllViews();

            Debug.Log($"Screenshot: {Path.GetFullPath(path)}");
        }
    }
}