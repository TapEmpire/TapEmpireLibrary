#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TapEmpire.Build
{
    public class IosPostProcessor
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
            project.SetBuildProperty(unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            project.WriteToFile(projectPath);

            Debug.Log("Updated Always Embed Swift Standard Libraries to NO for UnityFramework target.");
        }
    }
}
#endif