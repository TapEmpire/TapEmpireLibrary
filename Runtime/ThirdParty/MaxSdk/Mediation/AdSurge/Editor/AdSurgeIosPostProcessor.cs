#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace TapEmpire.Build
{
    public static class AdSurgeIosPostProcessor
    {
        private const string FrameworkName = "AdSurgeSDK.xcframework";
        private const int EmbedFrameworksPriority = 90;

        [PostProcessBuild(EmbedFrameworksPriority)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            string podsPath = Path.Combine(pathToBuiltProject, "Pods");
            if (!Directory.Exists(podsPath))
                return;

            string[] frameworks = Directory.GetDirectories(podsPath, FrameworkName, SearchOption.AllDirectories);
            if (frameworks.Length == 0)
                return;

            string frameworkPath = Path.GetRelativePath(pathToBuiltProject, frameworks[0]);

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string fileGuid = project.AddFile(frameworkPath, frameworkPath);
            project.AddFileToEmbedFrameworks(project.GetUnityMainTargetGuid(), fileGuid);
            project.WriteToFile(projectPath);

            Debug.Log($"[AdSurgeIosPostProcessor] Embedded {frameworkPath} into the Unity-iPhone target.");
        }
    }
}
#endif
