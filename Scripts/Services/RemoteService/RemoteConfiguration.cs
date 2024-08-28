using System.Collections.Generic;
using Firebase.RemoteConfig;

namespace TapEmpire.Services
{
    public sealed class RemoteConfiguration : IRemoteConfiguration
    {
        private readonly IDictionary<string, ConfigValue> _allValues;

        public RemoteConfiguration(IDictionary<string, ConfigValue> allValues)
        {
            _allValues = allValues;
        }

        public bool TryGetFloat(string key, out float value)
        {
            if (_allValues.TryGetValue(key, out var info))
            {
                value = (float)info.DoubleValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetString(string key, out string value)
        {
            if (_allValues.TryGetValue(key, out var info))
            {
                value = info.StringValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetBool(string key, out bool value)
        {
            if (_allValues.TryGetValue(key, out var info))
            {
                value = info.BooleanValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetInt(string key, out int value)
        {
            if (_allValues.TryGetValue(key, out var info))
            {
                value = (int)info.LongValue;
                return true;
            }

            value = default;
            return false;
        }

        public string GetString(string key, string defaultValue)
        {
            return TryGetString(key, out var value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue = default)
        {
            return TryGetInt(key, out var value) ? value : defaultValue;
        }
    }

    public sealed class EmptyRemoteConfiguration : IRemoteConfiguration
    {
        public bool TryGetFloat(string key, out float value)
        {
            value = default;
            return false;
        }

        public bool TryGetString(string key, out string value)
        {
            value = default;
            return false;
        }

        public bool TryGetBool(string key, out bool value)
        {
            value = default;
            return false;
        }

        public bool TryGetInt(string key, out int value)
        {
            value = default;
            return false;
        }

        public string GetString(string key, string defaultValue = default)
        {
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = default)
        {
            return defaultValue;
        }
    }
}