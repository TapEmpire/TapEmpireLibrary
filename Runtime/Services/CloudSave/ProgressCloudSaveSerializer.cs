#if TEL_CLOUD_SAVE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ProgressCloudSaveSerializer : ICloudSaveSerializer<ProgressSnapshot>
    {
        private readonly IProgressService _progressService;
        private readonly HashSet<string> _excludedKeys = new();

        public int ExcludedKeysCount => _excludedKeys.Count;

        public ProgressCloudSaveSerializer(IProgressService progressService, CloudSaveSettings settings)
        {
            _progressService = progressService;

            if (settings?.ExcludedKeys is not {Length: > 0} excluded) 
                return;
            foreach (var key in excluded)
            {
                if (!string.IsNullOrWhiteSpace(key)) 
                    _excludedKeys.Add(key);
            }
        }

        public ProgressSnapshot Export()
        {
            var snapshot = new ProgressSnapshot
            {
                SchemaVersion = ProgressSnapshot.CurrentSchemaVersion,
                UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeviceId = SystemInfo.deviceUniqueIdentifier,
            };

            ExportValues(_progressService.IntValuesDictionary, snapshot.IntValues);
            ExportValues(_progressService.BoolValuesDictionary, snapshot.BoolValues);
            ExportValues(_progressService.StringValuesDictionary, snapshot.StringValues);

            return snapshot;
        }

        public void Import(ProgressSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            ImportValues(snapshot.IntValues, _progressService.IntValuesDictionary);
            ImportValues(snapshot.BoolValues, _progressService.BoolValuesDictionary);
            ImportValues(snapshot.StringValues, _progressService.StringValuesDictionary);
        }

        private void ExportValues<T>(ICachedReactiveDictionary<T> dictionary, Dictionary<string, T> target)
        {
            foreach (var key in dictionary.GetCachedKeys())
            {
                if (_excludedKeys.Contains(key)) 
                    continue;
                if (dictionary.TryGetValue(key, out var value, canUseDefault: false)) 
                    target[key] = value;
            }
        }

        private void ImportValues<T>(Dictionary<string, T> values, ICachedReactiveDictionary<T> dictionary)
        {
            if (values == null) 
                return;
            foreach (var pair in values)
            {
                if (_excludedKeys.Contains(pair.Key)) continue;
                dictionary.SetValue(pair.Key, pair.Value);
            }
        }
    }
}
#endif
