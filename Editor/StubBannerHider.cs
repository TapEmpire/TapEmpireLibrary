using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TapEmpire.Editor
{
    [InitializeOnLoad]
    public static class StubBannerHider
    {
        private const string MenuPath = "TapEmpire/Tools/Hide Stub Banners";
        private const string EnabledKey = "TapEmpire.Tools.HideStubBanners";
        private const double ScanInterval = 0.5d;

        private static readonly string[] BannerNamePrefixes = { "BannerBottom", "BannerTop" };

        private static Scene _dontDestroyOnLoadScene;
        private static double _nextScanTime;

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, false);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        static StubBannerHider()
        {
            EditorApplication.update += OnUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            var enabled = !Enabled;
            Enabled = enabled;

            if (EditorApplication.isPlaying)
            {
                SetBannersHidden(enabled);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                _nextScanTime = 0d;
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                _dontDestroyOnLoadScene = default;
            }
        }

        private static void OnUpdate()
        {
            if (!Enabled || !EditorApplication.isPlaying)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup < _nextScanTime)
            {
                return;
            }

            _nextScanTime = EditorApplication.timeSinceStartup + ScanInterval;
            SetBannersHidden(true);
        }

        private static void SetBannersHidden(bool hidden)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    SetBannersHidden(scene, hidden);
                }
            }

            SetBannersHidden(GetDontDestroyOnLoadScene(), hidden);
        }

        private static void SetBannersHidden(Scene scene, bool hidden)
        {
            if (!scene.IsValid())
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (!IsStubBanner(root.name))
                {
                    continue;
                }

                if (SceneVisibilityManager.instance.IsHidden(root) == hidden)
                {
                    continue;
                }

                if (hidden)
                {
                    SceneVisibilityManager.instance.Hide(root, true);
                }
                else
                {
                    SceneVisibilityManager.instance.Show(root, true);
                }
            }
        }

        private static bool IsStubBanner(string name)
        {
            foreach (var prefix in BannerNamePrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static Scene GetDontDestroyOnLoadScene()
        {
            if (_dontDestroyOnLoadScene.IsValid())
            {
                return _dontDestroyOnLoadScene;
            }

            var probe = new GameObject(nameof(StubBannerHider))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            UnityEngine.Object.DontDestroyOnLoad(probe);
            _dontDestroyOnLoadScene = probe.scene;
            UnityEngine.Object.DestroyImmediate(probe);

            return _dontDestroyOnLoadScene;
        }
    }
}
