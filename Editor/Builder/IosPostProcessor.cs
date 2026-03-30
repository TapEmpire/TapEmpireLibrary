#if UNITY_IOS
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TapEmpire.Build
{
    public class IosPostProcessor
    {
        private const string UnifiedEntitlementsFileName = "Unity-iPhone.entitlements";

        [PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            string unityMainTarget = project.GetUnityMainTargetGuid();
            string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
            project.SetBuildProperty(unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            project.WriteToFile(projectPath);

            Debug.Log("Updated Always Embed Swift Standard Libraries to NO for UnityFramework target.");

#if TEL_CLOUD_SAVE
            EnableICloudCapability(project, projectPath, pathToBuiltProject, unityMainTarget);
            NormalizeEntitlements(projectPath, pathToBuiltProject);
#endif
            InjectToPlist(pathToBuiltProject);
        }

#if TEL_CLOUD_SAVE
        private static void EnableICloudCapability(PBXProject project, string projectPath, string pathToBuiltProject, string unityMainTarget)
        {
            const string entitlementsFileName = UnifiedEntitlementsFileName;
            string currentEntitlementsFileName = project.GetBuildPropertyForAnyConfig(unityMainTarget, "CODE_SIGN_ENTITLEMENTS");

            if (!string.IsNullOrEmpty(currentEntitlementsFileName) &&
                currentEntitlementsFileName != entitlementsFileName)
            {
                string currentEntitlementsPath = Path.Combine(pathToBuiltProject, currentEntitlementsFileName);
                string targetEntitlementsPath = Path.Combine(pathToBuiltProject, entitlementsFileName);

                if (File.Exists(currentEntitlementsPath))
                {
                    File.Copy(currentEntitlementsPath, targetEntitlementsPath, true);
                }
            }

            project.SetBuildProperty(unityMainTarget, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);
            project.WriteToFile(projectPath);

            string bundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            string iCloudContainer = $"iCloud.{bundleIdentifier}";

            ProjectCapabilityManager capabilityManager =
                new ProjectCapabilityManager(projectPath, entitlementsFileName, null, unityMainTarget);

            capabilityManager.AddiCloud(
                enableKeyValueStorage: false,
                enableiCloudDocument: true,
                enablecloudKit: true,
                addDefaultContainers: false,
                customContainers: new[] { iCloudContainer });
            capabilityManager.WriteToFile();

            Debug.Log($"Enabled iCloud Documents and CloudKit for container '{iCloudContainer}'.");
        }

        private static void NormalizeEntitlements(string projectPath, string pathToBuiltProject)
        {
            string unifiedEntitlementsPath = Path.Combine(pathToBuiltProject, UnifiedEntitlementsFileName);
            EnsureEntitlementsFileExists(unifiedEntitlementsPath);

            PlistDocument unifiedEntitlements = new PlistDocument();
            unifiedEntitlements.ReadFromFile(unifiedEntitlementsPath);

            foreach (string entitlementsPath in Directory.GetFiles(pathToBuiltProject, "*.entitlements"))
            {
                if (entitlementsPath == unifiedEntitlementsPath)
                {
                    continue;
                }

                PlistDocument sourceEntitlements = new PlistDocument();
                sourceEntitlements.ReadFromFile(entitlementsPath);
                MergeEntitlements(unifiedEntitlements.root, sourceEntitlements.root);
            }

            NormalizeICloudContainers(unifiedEntitlements.root);
            unifiedEntitlements.WriteToFile(unifiedEntitlementsPath);

            string projectContents = File.ReadAllText(projectPath);
            projectContents = Regex.Replace(
                projectContents,
                "CODE_SIGN_ENTITLEMENTS = \".*?\\.entitlements\";",
                $"CODE_SIGN_ENTITLEMENTS = \"{UnifiedEntitlementsFileName}\";");
            File.WriteAllText(projectPath, projectContents);
        }

        private static void EnsureEntitlementsFileExists(string entitlementsPath)
        {
            if (File.Exists(entitlementsPath))
            {
                return;
            }

            const string emptyEntitlements =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n" +
                "<plist version=\"1.0\">\n" +
                "  <dict>\n" +
                "  </dict>\n" +
                "</plist>\n";

            File.WriteAllText(entitlementsPath, emptyEntitlements);
        }

        private static void MergeEntitlements(PlistElementDict target, PlistElementDict source)
        {
            foreach (var pair in source.values)
            {
                target.values[pair.Key] = pair.Value;
            }
        }

        private static void NormalizeICloudContainers(PlistElementDict entitlements)
        {
            string bundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            string iCloudContainer = $"iCloud.{bundleIdentifier}";

            SetSingleStringArray(entitlements, "com.apple.developer.icloud-container-identifiers", iCloudContainer);
            SetSingleStringArray(entitlements, "com.apple.developer.ubiquity-container-identifiers", iCloudContainer);
        }

        private static void SetSingleStringArray(PlistElementDict dict, string key, string value)
        {
            dict.values.Remove(key);
            PlistElementArray array = dict.CreateArray(key);
            array.AddString(value);
        }
#endif

        private static void InjectToPlist(string pathToBuiltProject)
        {
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            const string trackingKey = "NSUserTrackingUsageDescription";
            const string trackingMessage = "We use your data to deliver personalized ads.";

            if (!rootDict.values.ContainsKey(trackingKey))
            {
                rootDict.SetString(trackingKey, trackingMessage);
            }

            const string encryptionKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptionKey, false);

            plist.WriteToFile(plistPath);
        }
    }
}
#endif
