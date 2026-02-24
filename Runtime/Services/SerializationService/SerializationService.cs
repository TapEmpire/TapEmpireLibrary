using System.Collections.Generic;
using Zenject;
using R3;
using UnityEngine;
using System;
using TapEmpire.Utility;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class SerializationService : Initializable, ISerializationService
    {
        public static readonly string ConfigNameKey = "ConfigName";

        [SerializeReference]
        private List<IRemoteSerializable> _serializables = new();

#if UNITY_EDITOR
        [Header("Editor Remote Config Override")]
        [Tooltip("Enable to use editor overrides instead of default values")]
        [SerializeField]
        private bool _useEditorOverrides = false;

        [Tooltip("Remote config key-value overrides for editor testing")]
        [SerializeField]
        private List<EditorRemoteConfigEntry> _editorOverrides = new();

        [Button("Copy Current Settings to Overrides")]
        private void CopyCurrentSettingsToOverrides()
        {
            _editorOverrides.Clear();
            foreach (var serializable in _serializables)
            {
                if (serializable == null) continue;

                _editorOverrides.Add(new EditorRemoteConfigEntry
                {
                    Key = serializable.TokenName,
                    Value = serializable.SerializeJson()
                });
            }
            Debug.Log($"Copied {_editorOverrides.Count} config entries to editor overrides");
        }

        [Button("Log All Override Keys")]
        private void LogOverrideKeys()
        {
            foreach (var entry in _editorOverrides)
            {
                Debug.Log($"[{entry.Key}]: {entry.Value}");
            }
        }
#endif

        private Dictionary<string, IRemoteSerializable> _serializableDictionary = new();

        private IFirebaseService _firebaseService = null;
        private IProgressService _progressService = null;

        private IDisposable _disposable = null;

        [Inject]
        private void Construct(IFirebaseService firebaseService, IProgressService progressService)
        {
            _progressService = progressService;
            _firebaseService = firebaseService;

            _serializableDictionary.Clear();
            _serializables.ForEach(serializable => _serializableDictionary.Add(serializable.TokenName, serializable));

            // _disposable = _firebaseService.IsLoaded.Subscribe(OnLoaded);
        }

        protected override async UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            OnLoaded(true);
            await UniTask.CompletedTask;
        }

        public string GetSerializedConfig(string configName)
        {
            return _serializableDictionary[configName].SerializeJson();
        }

        private void OnLoaded(bool isLoaded)
        {
            if (!isLoaded) return;

            // _disposable.Dispose();
#if UNITY_EDITOR
            if (_useEditorOverrides)
            {
                var editorConfig = new EditorRemoteConfiguration(_editorOverrides);
                _progressService.SetRemoteConfigName(editorConfig.GetString(ConfigNameKey, "editorOverride"));
                ApplyRemoteConfig(editorConfig);
            }
            else
            {
                _progressService.SetRemoteConfigName("unityEditor");
            }
#else
            _progressService.SetRemoteConfigName(_firebaseService.RemoteConfiguration.GetString(ConfigNameKey, "default"));
            ApplyRemoteConfig(_firebaseService.RemoteConfiguration);
#endif
        }

        private void ApplyRemoteConfig(IRemoteConfiguration remoteConfig)
        {
            _serializableDictionary.ForEach(x =>
            {
                try
                {
                    var jsonString = remoteConfig.GetString(x.Key, string.Empty);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        x.Value.DeserializeJson(JToken.Parse(jsonString));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            // _disposable.Dispose();
            _serializableDictionary.Clear();
        }
    }
}
