//
//  iOSPodfilePostProcessor.cs
//
//  Adds post_install hook to Podfile to fix BUILD_LIBRARY_FOR_DISTRIBUTION issues
//  for problematic pods like AppMetricaLibraryAdapter.
//  Also removes Local Network permission request.
//

#if (UNITY_IOS || UNITY_IPHONE) && UNITY_6000
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TapEmpire.Editor
{
    public static class iOSPodfilePostProcessor
    {
        private const string NSBonjourServicesKey = "NSBonjourServices";
        private const string NSLocalNetworkUsageDescriptionKey = "NSLocalNetworkUsageDescription";
        private const string PostInstallBlock = @"
post_install do |installer|
  problematic_targets = ['AppMetricaLibraryAdapter']

  # Fix xcconfig files
  installer.pods_project.targets.each do |target|
    if problematic_targets.include?(target.name)
      target.build_configurations.each do |config|
        xcconfig_path = config.base_configuration_reference&.real_path
        if xcconfig_path && File.exist?(xcconfig_path)
          xcconfig_content = File.read(xcconfig_path)
          new_content = xcconfig_content.gsub(/BUILD_LIBRARY_FOR_DISTRIBUTION = YES/, 'BUILD_LIBRARY_FOR_DISTRIBUTION = NO')
          File.write(xcconfig_path, new_content) if new_content != xcconfig_content
        end
        config.build_settings['BUILD_LIBRARY_FOR_DISTRIBUTION'] = 'NO'
      end
    end
  end
end
";

        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            RemoveLocalNetworkPermission(buildPath);

            if (FixPodfile(buildPath))
            {
                RunPodInstall(buildPath);
            }
        }

        private static void RemoveLocalNetworkPermission(string buildPath)
        {
            var plistPath = Path.Combine(buildPath, "Info.plist");

            if (!File.Exists(plistPath))
            {
                UnityEngine.Debug.LogWarning("[iOSPodfilePostProcessor] Info.plist not found at: " + plistPath);
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            bool modified = false;

            if (plist.root.values.ContainsKey(NSBonjourServicesKey))
            {
                plist.root.values.Remove(NSBonjourServicesKey);
                modified = true;
                UnityEngine.Debug.Log("[iOSPodfilePostProcessor] Removed NSBonjourServices from Info.plist");
            }

            if (plist.root.values.ContainsKey(NSLocalNetworkUsageDescriptionKey))
            {
                plist.root.values.Remove(NSLocalNetworkUsageDescriptionKey);
                modified = true;
                UnityEngine.Debug.Log("[iOSPodfilePostProcessor] Removed NSLocalNetworkUsageDescription from Info.plist");
            }

            if (modified)
            {
                plist.WriteToFile(plistPath);
                UnityEngine.Debug.Log("[iOSPodfilePostProcessor] Info.plist updated - Local Network permission removed");
            }
            else
            {
                UnityEngine.Debug.Log("[iOSPodfilePostProcessor] No Local Network keys found in Info.plist");
            }
        }

        private static bool FixPodfile(string buildPath)
        {
            var podfilePath = Path.Combine(buildPath, "Podfile");

            if (!File.Exists(podfilePath))
            {
                UnityEngine.Debug.LogWarning("[iOSPodfilePostProcessor] Podfile not found at: " + podfilePath);
                return false;
            }

            var content = File.ReadAllText(podfilePath);
            
            if (Regex.IsMatch(content, @"post_install\s+do\s+\|installer\|"))
            {
                if (content.Contains("AppMetricaLibraryAdapter") && content.Contains("BUILD_LIBRARY_FOR_DISTRIBUTION"))
                {
                    UnityEngine.Debug.Log("[iOSPodfilePostProcessor] Podfile already contains the fix, skipping");
                    return false;
                }

                UnityEngine.Debug.LogWarning("[iOSPodfilePostProcessor] Podfile already has a post_install block. Please merge manually:\n" + PostInstallBlock);
                return false;
            }
            
            content = content.TrimEnd() + "\n" + PostInstallBlock;

            File.WriteAllText(podfilePath, content);
            UnityEngine.Debug.Log("[iOSPodfilePostProcessor] Added post_install hook to Podfile for BUILD_LIBRARY_FOR_DISTRIBUTION fix");
            return true;
        }

        private static readonly string[] PodPaths = new string[]
        {
            "/opt/homebrew/bin/pod",      // Apple Silicon Homebrew
            "/usr/local/bin/pod",          // Intel Homebrew
            "/usr/bin/pod"
        };

        private static string FindPodExecutable()
        {
            foreach (var path in PodPaths)
            {
                if (File.Exists(path))
                    return path;
            }
            return null;
        }

        private static void RunPodInstall(string buildPath)
        {
            var podPath = FindPodExecutable();
            if (podPath == null)
            {
                UnityEngine.Debug.LogError("[iOSPodfilePostProcessor] CocoaPods not found. Tried paths:\n" + string.Join("\n", PodPaths));
                return;
            }

            UnityEngine.Debug.Log($"[iOSPodfilePostProcessor] Running pod install using {podPath}...");

            var process = new Process();
            process.StartInfo.FileName = podPath;
            process.StartInfo.Arguments = "install";
            process.StartInfo.WorkingDirectory = buildPath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
            process.StartInfo.EnvironmentVariables["LC_ALL"] = "en_US.UTF-8";

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                UnityEngine.Debug.Log("[iOSPodfilePostProcessor] pod install completed successfully\n" + output);
            }
            else
            {
                UnityEngine.Debug.LogError("[iOSPodfilePostProcessor] pod install failed:\n" + error + "\n" + output);
            }
        }
    }
}
#endif
