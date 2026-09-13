using System;
using System.Linq;
using AdjustSdk;
using LunarConsolePlugin;
using TapEmpire.Services;
using TapEmpire.Settings;
using TEL.Utilities;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TapEmpire.Build
{
    using Utility;

    public static class GameBuilder
    {
        // GameCI: -executeMethod TapEmpire.Build.GameBuilder.BuildFromCommandLine
        //   -buildConfig Release -platform Ios -buildVersion 1.4.0 -buildNumber 137 [-buildPath <path>]
        public static void BuildFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var config = ParseEnum(args, "-buildConfig", Configuration.Debug);
            var platform = ParseEnum(args, "-platform", PlatformType.Android);
            var version = GetArg(args, "-buildVersion");
            var buildNumber = int.TryParse(GetArg(args, "-buildNumber"), out var n) ? n : 0;
            var buildPath = GetArg(args, "-buildPath");
            var appName = GetArg(args, "-appName");
            var gradleDir = GetArg(args, "-gradleDir");
            var jdkPath = GetArg(args, "-jdkPath");

            if (platform == PlatformType.Android)
            {
                ApplyAndroidKeystoreFromEnv();

                if (!string.IsNullOrEmpty(gradleDir))
                {
                    UnityEditor.Android.AndroidExternalToolsSettings.Gradle.path = gradleDir;
                    Debug.Log($"[GameBuilder] Custom Gradle → {UnityEditor.Android.AndroidExternalToolsSettings.Gradle.path}");
                }

                if (!string.IsNullOrEmpty(jdkPath))
                {
                    var gradleUserHome = System.Environment.GetEnvironmentVariable("GRADLE_USER_HOME")
                        ?? System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("HOME") ?? "", ".gradle");
                    System.IO.Directory.CreateDirectory(gradleUserHome);
                    var props = System.IO.Path.Combine(gradleUserHome, "gradle.properties");
                    System.IO.File.AppendAllText(props, $"org.gradle.java.home={jdkPath}{System.Environment.NewLine}");
                    Debug.Log($"[GameBuilder] org.gradle.java.home → {jdkPath} (wrote {props})");
                }
            }

            Build(config, platform, version, buildNumber, buildPath, appName);
        }

        static void ApplyAndroidKeystoreFromEnv()
        {
            var keystoreName = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_NAME");
            if (string.IsNullOrEmpty(keystoreName)) return;

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS");
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_NAME");
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_PASS");
        }

        public static void Build(Configuration config, PlatformType platform,
                                 string version = null, int buildNumber = 0,
                                 string buildPath = null, string appName = null)
        {
            Apply(config, platform);

            if (!string.IsNullOrEmpty(version))
            {
                if (version.StartsWith("v") || version.StartsWith("V"))
                    version = version.Substring(1);
                PlayerSettings.bundleVersion = version;
            }

            if (buildNumber > 0)
            {
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            }

            var target = platform == PlatformType.Android ? BuildTarget.Android : BuildTarget.iOS;
            var group = platform == PlatformType.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
            if (EditorUserBuildSettings.activeBuildTarget != target)
                EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);

            if (platform == PlatformType.Android)
            {
                EditorUserBuildSettings.buildAppBundle = config == Configuration.Release;
                EditorUserBuildSettings.androidCreateSymbols = config == Configuration.Release
                    ? AndroidCreateSymbols.Public
                    : AndroidCreateSymbols.Disabled;
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                targetGroup = group,
                locationPathName = buildPath ?? DefaultLocation(config, platform, appName),
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[GameBuilder] {summary.result}: {options.locationPathName} " +
                      $"({summary.totalErrors} errors, {summary.totalWarnings} warnings)");

            if (summary.result != BuildResult.Succeeded)
                throw new Exception($"[GameBuilder] Build failed: {summary.result}");
        }

        public static void Apply(Configuration config, PlatformType platform)
        {
            var paths = ProjectPathSettings.Instance;
            ApplyBuildMode(config, paths);
            ApplyPlatform(platform, paths);
            ApplyActions(config, paths);
        }

        private static string DefaultLocation(Configuration config, PlatformType platform, string appName = null)
        {
            if (platform == PlatformType.Ios)
                return "Builds/iOS";

            var ext = config == Configuration.Release ? ".aab" : ".apk";
            var rawName = string.IsNullOrEmpty(appName) ? Application.productName : appName;
            var safeName = rawName.Replace(" ", "");
            var name = $"{safeName}" +
                       $"-{config.ToString().ToLower()}" +
                       $"-{PlayerSettings.bundleVersion}" +
                       $"-build{PlayerSettings.Android.bundleVersionCode}{ext}";
            return $"Builds/{name}";
        }

        private static void ApplyBuildMode(Configuration config, ProjectPathSettings paths)
        {
            var systemSettings = AssetDatabase.LoadAssetAtPath<SystemSettings>(paths.SystemSettingsPath);
            systemSettings.Debug = config == Configuration.Debug;
            systemSettings.IgnoreConnection &= config == Configuration.Debug;
            EditorUtility.SetDirty(systemSettings);

            var gameSettings = AssetDatabase.LoadAssetAtPath<GameSettings>(paths.GameSettingsPath);
            gameSettings.AutoRestartLevel &= config == Configuration.Debug;
            EditorUtility.SetDirty(gameSettings);

            var attributionSettings = AssetDatabase.LoadAssetAtPath<AttributionSettings>($"{paths.DefaultScriptablesPath}/AttributionSettings.asset");
            attributionSettings.Environment = config == Configuration.Debug
                ? AdjustEnvironment.Sandbox
                : AdjustEnvironment.Production;
            EditorUtility.SetDirty(attributionSettings);

            var adsConfig = AssetDatabase.LoadAssetAtPath<AdsConfig>($"{paths.DefaultScriptablesPath}/AdsConfig.asset");
            adsConfig.TestMode = config == Configuration.Debug;
            adsConfig.SkipAds &= config == Configuration.Debug;
            EditorUtility.SetDirty(adsConfig);

            var console = UnityEngine.Object.FindObjectOfType<LunarConsole>(true);
            if (console)
            {
                console.gameObject.SetActive(config == Configuration.Debug);
                EditorTools.SetDirty(console);
            }
        }

        private static void ApplyPlatform(PlatformType platform, ProjectPathSettings paths)
        {
            var buildSettings = AssetDatabase.LoadAssetAtPath<GameBuildSettings>(paths.GameBuildSettingsPath);
            var platformData = platform == PlatformType.Android ? buildSettings.Android : buildSettings.Ios;

            var attributionSettings = AssetDatabase.LoadAssetAtPath<AttributionSettings>($"{paths.DefaultScriptablesPath}/AttributionSettings.asset");
            attributionSettings.AppToken = platformData.Adjust.AppToken;
            EditorUtility.SetDirty(attributionSettings);

            var adsConfig = AssetDatabase.LoadAssetAtPath<AdsConfig>($"{paths.DefaultScriptablesPath}/AdsConfig.asset");
            adsConfig.Admob.CopyFrom(platformData.GoogleAds);
            adsConfig.AppLovin.CopyFrom(platformData.ApplovinAds);
            EditorUtility.SetDirty(adsConfig);

            var servicesInstaller = AssetDatabase.LoadAssetAtPath<ServicesInstaller>($"{paths.DefaultScriptablesPath}/ServicesInstaller.asset");
            var iapService = servicesInstaller.GetService<IIapService>();
            ReflectionUtility.SetPrivateField(iapService as object, "<AdjustPurchaseToken>k__BackingField", platformData.Adjust.PurchaseToken);
            var analyticsService = servicesInstaller.GetService<IAnalyticsService>();
            ReflectionUtility.SetPrivateField(analyticsService as object, "<AdjustEventToken>k__BackingField", platformData.Adjust.EventToken);
            EditorUtility.SetDirty(servicesInstaller);

            AppLovinSettings.Instance.AdMobAndroidAppId = buildSettings.Android.GoogleAds.AppKey;
            AppLovinSettings.Instance.AdMobIosAppId = buildSettings.Ios.GoogleAds.AppKey;
            EditorUtility.SetDirty(AppLovinSettings.Instance);

            var googleAdsSettings = EditorCustomUtility.LoadFirstAsset("GoogleMobileAdsSettings");
            ReflectionUtility.SetPrivateField<string>(googleAdsSettings, "adMobAndroidAppId", buildSettings.Android.GoogleAds.AppKey);
            ReflectionUtility.SetPrivateField<string>(googleAdsSettings, "adMobIOSAppId", buildSettings.Ios.GoogleAds.AppKey);
            EditorUtility.SetDirty(googleAdsSettings);
        }

        private static void ApplyActions(Configuration config, ProjectPathSettings paths)
        {
            var buildSettings = AssetDatabase.LoadAssetAtPath<GameBuildSettings>(paths.GameBuildSettingsPath);
            var isDebug = config == Configuration.Debug;
            buildSettings.BuildActions.ForEach(action => action.Apply(isDebug));
        }

        private static string GetArg(string[] args, string key)
        {
            var index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private static T ParseEnum<T>(string[] args, string key, T fallback) where T : struct
        {
            var value = GetArg(args, key);
            return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : fallback;
        }
    }
}
