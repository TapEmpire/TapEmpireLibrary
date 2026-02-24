using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Services
{
    [Serializable]
    public class EditorRemoteConfigEntry
    {
        [HorizontalGroup("Row", Width = 0.3f)]
        [HideLabel]
        public string Key;

        [HorizontalGroup("Row")]
        [HideLabel]
        [TextArea(1, 10)]
        public string Value;
    }

    public sealed class EditorRemoteConfiguration : IRemoteConfiguration
    {
        private readonly Dictionary<string, string> _values = new();

        public EditorRemoteConfiguration(IEnumerable<EditorRemoteConfigEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    _values[entry.Key] = entry.Value;
                }
            }
        }

        public bool TryGetFloat(string key, out float value)
        {
            if (_values.TryGetValue(key, out var str) && float.TryParse(str, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetString(string key, out string value)
        {
            return _values.TryGetValue(key, out value);
        }

        public bool TryGetBool(string key, out bool value)
        {
            if (_values.TryGetValue(key, out var str) && bool.TryParse(str, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetInt(string key, out int value)
        {
            if (_values.TryGetValue(key, out var str) && int.TryParse(str, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        public string GetString(string key, string defaultValue)
        {
            return TryGetString(key, out var value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            return TryGetInt(key, out var value) ? value : defaultValue;
        }
    }
}
