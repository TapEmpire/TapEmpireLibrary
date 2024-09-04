using System.Collections.Generic;
using Zenject;
using R3;
using UnityEngine;
using System;
using TapEmpire.Utility;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class SerializationService : Initializable, ISerializationService
    {
        public static readonly string ConfigNameKey = "ConfigName";

        [SerializeReference]
        private List<IRemoteSerializable> _serializables = new();
        
        private Dictionary<string, IRemoteSerializable> _serializableDictionary = new();

        private IFirebaseService _firebaseService = null;
        private IProgressService _progressService = null;

        private IDisposable _disposable = null;

        [Inject]
        private void Construct(IFirebaseService firebaseService, IProgressService progressService)
        {
            _progressService = progressService;
            _firebaseService = firebaseService;

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
#if !UNITY_EDITOR
            _progressService.SetRemoteConfigName("unityEditor");
#else
            _progressService.SetRemoteConfigName(_firebaseService.RemoteConfiguration.GetString(ConfigNameKey, string.Empty));
            _serializableDictionary.ForEach(x =>
            {
                try
                {
                    var jsonString = _firebaseService.RemoteConfiguration.GetString(x.Key, string.Empty);
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
#endif
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            // _disposable.Dispose();
            _serializableDictionary.Clear();
        }
    }
}
