using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using AdjustSdk;
using LunarConsolePlugin;
using TapEmpire.Settings;
using TEL.Utilities;
using TapEmpire.Services;

namespace TapEmpire.Build
{
    using Utility;

    public class GameBuilderWindow : OdinEditorWindow
    {
        [SerializeField]
        private string _buildVersion;

        [EnumToggleButtons, BoxGroup("BuildMode")]
        public Configuration SelectedBuildConfig;

        [EnumToggleButtons, BoxGroup("BuildMode")]
        public PlatformType PlatformType;

        [ToggleLeft, BoxGroup("Keystore")]
        public bool UseCustomKeystore;

        [ShowIf("UseCustomKeystore"), BoxGroup("Keystore")]
        public TextAsset KeystoreSettings;

        private ProjectPathSettings _projectPathSettings = null;

        [MenuItem("TapEmpire/Builder")]
        public static void ShowWindow()
        {
            GetWindow<GameBuilderWindow>("Builder").Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _buildVersion = PlayerSettings.bundleVersion;
            _projectPathSettings = ProjectPathSettings.Instance;
        }

        [Button("Build Game", ButtonSizes.Large), PropertySpace(SpaceBefore = 10), PropertyOrder(100)]
        private void BuildGame()
        {
            if (UseCustomKeystore && KeystoreSettings != null)
            {
                ApplyKeystoreSettings();
            }
            else
            {
                ClearKeystoreSettings();
            }
            Apply();
            PlayerSettings.bundleVersion = _buildVersion;

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var productName = Application.productName;
            EditorUserBuildSettings.buildAppBundle = SelectedBuildConfig == Configuration.Release;
            EditorUserBuildSettings.androidCreateSymbols = SelectedBuildConfig == Configuration.Release
                ? AndroidCreateSymbols.Public
                : AndroidCreateSymbols.Disabled;
            var extension = (SelectedBuildConfig == Configuration.Release) ? ".aab" : ".apk";
            var buildName = $"{productName}_{_buildVersion}_{SelectedBuildConfig.ToString().ToLower()}{extension}";
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"Builds/{buildName}",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"Building {buildName}");
        }

        [Button, BoxGroup("BuildMode")]
        private void Apply()
        {
            ApplyBuildMode();
            ApplyPlatform();
            ApplyActions();
        }

        private void ApplyBuildMode()
        {
            var startSettings = AssetDatabase.LoadAssetAtPath<GameStartSettings>(_projectPathSettings.GameStartSettingsPath);
            startSettings.Debug = SelectedBuildConfig == Configuration.Debug;
            startSettings.SkipInters &= SelectedBuildConfig == Configuration.Debug;
            startSettings.AutoRestartLevel &= SelectedBuildConfig == Configuration.Debug;
            startSettings.IgnoreConnection &= SelectedBuildConfig == Configuration.Debug;
            EditorUtility.SetDirty(startSettings);

            var adjust = AssetDatabase.LoadAssetAtPath<Adjust>($"{_projectPathSettings.DefaultServicesPath}/Adjust Variant.prefab");
            adjust.environment = SelectedBuildConfig == Configuration.Debug
                ? AdjustEnvironment.Sandbox
                : AdjustEnvironment.Production;
            EditorUtility.SetDirty(adjust);

            var adsManager = AssetDatabase.LoadAssetAtPath<AdsManager>($"{_projectPathSettings.DefaultServicesPath}/AdsManager Variant.prefab");
            adsManager.TestAds = SelectedBuildConfig == Configuration.Debug;
            EditorUtility.SetDirty(adsManager);

            var console = FindObjectOfType<LunarConsole>(true);
            if (console)
            {
                console.gameObject.SetActive(SelectedBuildConfig == Configuration.Debug);
                EditorTools.SetDirty(console);
            }
        }

        private void ApplyPlatform()
        {
            var buildSettings = AssetDatabase.LoadAssetAtPath<GameBuildSettings>(_projectPathSettings.GameBuildSettingsPath);
            var platformData = PlatformType == PlatformType.Android ? buildSettings.Android : buildSettings.Ios;

            var adjust = AssetDatabase.LoadAssetAtPath<Adjust>($"{_projectPathSettings.DefaultServicesPath}/Adjust Variant.prefab");
            adjust.appToken = platformData.Adjust.AppToken;
            EditorUtility.SetDirty(adjust);

            var adsManager = AssetDatabase.LoadAssetAtPath<AdsManager>($"{_projectPathSettings.DefaultServicesPath}/AdsManager Variant.prefab");

            adsManager.AppID = platformData.GoogleAds.AppKey;
            adsManager.AppOpenID = platformData.GoogleAds.AppOpenId;
            adsManager.BannerID = platformData.GoogleAds.BannerId;
            adsManager.MrecID = platformData.GoogleAds.MrecId;
            adsManager.InterstitialID = platformData.GoogleAds.InterstitialId;
            adsManager.RewardedID = platformData.GoogleAds.RewardedId;

            adsManager.MaxSDKKey = platformData.ApplovinAds.AppKey;
            adsManager.MaxBanner = platformData.ApplovinAds.BannerId;
            adsManager.MaxInterstitial = platformData.ApplovinAds.InterstitialId;
            adsManager.MaxRewarded = platformData.ApplovinAds.RewardedId;

            EditorUtility.SetDirty(adsManager);

            var servicesInstaller = AssetDatabase.LoadAssetAtPath<ServicesInstaller>($"{_projectPathSettings.DefaultScriptablesPath}/ServicesInstaller.asset");

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

        private void ApplyActions()
        {
            var buildSettings = AssetDatabase.LoadAssetAtPath<GameBuildSettings>(_projectPathSettings.GameBuildSettingsPath);
            var isDebug = SelectedBuildConfig == Configuration.Debug;
            buildSettings.BuildActions.ForEach(action => action.Apply(isDebug));
        }

        [Button, BoxGroup("Keystore"), ShowIf(nameof(UseCustomKeystore))]
        private void ApplyKeystoreSettings()
        {
            try
            {
                var keystoreData = JsonUtility.FromJson<KeystoreJsonData>(KeystoreSettings.text);
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = keystoreData.keystorePath;
                PlayerSettings.Android.keystorePass = keystoreData.keystorePass;
                PlayerSettings.Android.keyaliasName = keystoreData.keyAlias;
                PlayerSettings.Android.keyaliasPass = keystoreData.keyPass;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing keystore JSON: " + ex.Message);
            }
        }

        private void ClearKeystoreSettings()
        {
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keyaliasPass = "";
            Debug.Log("Custom keystore settings have been cleared.");
        }
    }
}