using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ProgressSnapshotMapper
    {
        private readonly IProgressService _progressService;
        private readonly Dictionary<string, CloudSaveTrackedValueType> _trackedKeyTypes = new();
        private string[] _trackedKeys = Array.Empty<string>();

        public int TrackedKeysCount => _trackedKeys.Length;

        public ProgressSnapshotMapper(IProgressService progressService, CloudSaveSettings settings)
        {
            _progressService = progressService;
            BuildTrackedKeyConfiguration(settings);
        }

        public ProgressSnapshot Export()
        {
            var snapshot = new ProgressSnapshot
            {
                SchemaVersion = ProgressSnapshot.CurrentSchemaVersion,
                UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeviceId = SystemInfo.deviceUniqueIdentifier,
            };

            if (_trackedKeys.Length == 0)
            {
                return snapshot;
            }

            foreach (var key in _trackedKeys)
            {
                switch (ResolveType(key))
                {
                    case CloudSaveTrackedValueType.String:
                        if (_progressService.StringValuesDictionary.TryGetValue(key, out var stringValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = stringValue;
                        }
                        else if (_progressService.IntValuesDictionary.TryGetValue(key, out var legacyIntValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = legacyIntValue.ToString();
                        }
                        else if (_progressService.BoolValuesDictionary.TryGetValue(key, out var legacyBoolValue, canUseDefault: false))
                        {
                            snapshot.StringValues[key] = legacyBoolValue.ToString();
                        }
                        break;

                    case CloudSaveTrackedValueType.Int:
                        if (_progressService.IntValuesDictionary.TryGetValue(key, out var intValue, canUseDefault: false))
                        {
                            snapshot.IntValues[key] = intValue;
                        }
                        break;

                    case CloudSaveTrackedValueType.Bool:
                        if (_progressService.BoolValuesDictionary.TryGetValue(key, out var boolValue, canUseDefault: false))
                        {
                            snapshot.BoolValues[key] = boolValue;
                        }
                        break;

                    default:
                        ExportFallback(snapshot, key);
                        break;
                }
            }

            return snapshot;
        }

        public void Import(ProgressSnapshot snapshot)
        {
            if (snapshot == null || _trackedKeys.Length == 0)
            {
                return;
            }

            var incomingIntValues = snapshot.IntValues ?? new Dictionary<string, int>();
            var incomingBoolValues = snapshot.BoolValues ?? new Dictionary<string, bool>();
            var incomingStringValues = snapshot.StringValues ?? new Dictionary<string, string>();

            foreach (var key in _trackedKeys)
            {
                switch (ResolveType(key))
                {
                    case CloudSaveTrackedValueType.String:
                        ImportString(key, incomingStringValues, incomingIntValues, incomingBoolValues);
                        break;

                    case CloudSaveTrackedValueType.Int:
                        ImportInt(key, incomingIntValues);
                        break;

                    case CloudSaveTrackedValueType.Bool:
                        ImportBool(key, incomingBoolValues);
                        break;

                    default:
                        ImportFallback(key, incomingStringValues, incomingIntValues, incomingBoolValues);
                        break;
                }
            }
        }

        private void BuildTrackedKeyConfiguration(CloudSaveSettings settings)
        {
            _trackedKeyTypes.Clear();

            if (settings?.TrackedKeyTypes is { Length: > 0 } typedKeys)
            {
                foreach (var trackedKey in typedKeys)
                {
                    if (string.IsNullOrWhiteSpace(trackedKey.Key))
                    {
                        continue;
                    }

                    _trackedKeyTypes[trackedKey.Key] = trackedKey.ValueType;
                }
            }

            var keys = new List<string>(_trackedKeyTypes.Count);
            foreach (var pair in _trackedKeyTypes)
            {
                keys.Add(pair.Key);
            }

            _trackedKeys = keys.ToArray();
        }

        private CloudSaveTrackedValueType ResolveType(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return CloudSaveTrackedValueType.Unknown;
            }

            return _trackedKeyTypes.TryGetValue(key, out var valueType)
                ? valueType
                : CloudSaveTrackedValueType.Unknown;
        }

        private void ExportFallback(ProgressSnapshot snapshot, string key)
        {
            if (_progressService.StringValuesDictionary.TryGetValue(key, out var fallbackString, canUseDefault: false))
            {
                snapshot.StringValues[key] = fallbackString;
            }
            else if (_progressService.IntValuesDictionary.TryGetValue(key, out var fallbackInt, canUseDefault: false))
            {
                snapshot.IntValues[key] = fallbackInt;
            }
            else if (_progressService.BoolValuesDictionary.TryGetValue(key, out var fallbackBool, canUseDefault: false))
            {
                snapshot.BoolValues[key] = fallbackBool;
            }
        }

        private void ImportString(string key,
            Dictionary<string, string> stringValues,
            Dictionary<string, int> intValues,
            Dictionary<string, bool> boolValues)
        {
            if (stringValues.TryGetValue(key, out var stringValue))
            {
                _progressService.StringValuesDictionary.SetValue(key, stringValue, save: false);
            }
            else if (intValues.TryGetValue(key, out var legacyIntValue))
            {
                _progressService.StringValuesDictionary.SetValue(key, legacyIntValue.ToString(), save: false);
            }
            else if (boolValues.TryGetValue(key, out var legacyBoolValue))
            {
                _progressService.StringValuesDictionary.SetValue(key, legacyBoolValue.ToString(), save: false);
            }
            else
            {
                _progressService.StringValuesDictionary.DeleteKey(key);
            }
        }

        private void ImportInt(string key, Dictionary<string, int> intValues)
        {
            if (intValues.TryGetValue(key, out var intValue))
            {
                _progressService.IntValuesDictionary.SetValue(key, intValue, save: false);
            }
            else
            {
                _progressService.IntValuesDictionary.DeleteKey(key);
            }
        }

        private void ImportBool(string key, Dictionary<string, bool> boolValues)
        {
            if (boolValues.TryGetValue(key, out var boolValue))
            {
                _progressService.BoolValuesDictionary.SetValue(key, boolValue, save: false);
            }
            else
            {
                _progressService.BoolValuesDictionary.DeleteKey(key);
            }
        }

        private void ImportFallback(string key,
            Dictionary<string, string> stringValues,
            Dictionary<string, int> intValues,
            Dictionary<string, bool> boolValues)
        {
            if (stringValues.TryGetValue(key, out var fallbackString))
            {
                _progressService.StringValuesDictionary.SetValue(key, fallbackString, save: false);
            }
            else if (intValues.TryGetValue(key, out var fallbackInt))
            {
                _progressService.IntValuesDictionary.SetValue(key, fallbackInt, save: false);
            }
            else if (boolValues.TryGetValue(key, out var fallbackBool))
            {
                _progressService.BoolValuesDictionary.SetValue(key, fallbackBool, save: false);
            }
            else
            {
                _progressService.StringValuesDictionary.DeleteKey(key);
                _progressService.IntValuesDictionary.DeleteKey(key);
                _progressService.BoolValuesDictionary.DeleteKey(key);
            }
        }
    }
}