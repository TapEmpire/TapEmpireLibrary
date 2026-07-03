using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

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

        [ToggleLeft, BoxGroup("BuildMode"), Tooltip("Off = skip the Addressables content build and reuse the existing bundles/catalog.")]
        public bool BuildAddressables = true;

        [ToggleLeft, BoxGroup("Keystore")]
        public bool UseCustomKeystore;

        [ShowIf("UseCustomKeystore"), BoxGroup("Keystore")]
        public TextAsset KeystoreSettings;

        [MenuItem("TapEmpire/Builder")]
        public static void ShowWindow()
        {
            GetWindow<GameBuilderWindow>("Builder").Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _buildVersion = PlayerSettings.bundleVersion;
        }

        [Button("Build Game", ButtonSizes.Large), PropertySpace(SpaceBefore = 10), PropertyOrder(100)]
        private void BuildGame()
        {
            if (UseCustomKeystore && KeystoreSettings != null)
                ApplyKeystoreSettings();
            else
                ClearKeystoreSettings();

            GameBuilder.Build(SelectedBuildConfig, PlatformType, _buildVersion, buildAddressables: BuildAddressables);
        }

        [Button, BoxGroup("BuildMode")]
        private void Apply()
        {
            GameBuilder.Apply(SelectedBuildConfig, PlatformType);
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
