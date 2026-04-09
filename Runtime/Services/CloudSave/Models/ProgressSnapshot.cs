#if TEL_CLOUD_SAVE
using System;
using System.Collections.Generic;

namespace TapEmpire.Services
{
    [Serializable]
    public class ProgressSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long UpdatedAtUnixMs;
        public string DeviceId = string.Empty;

        public Dictionary<string, int> IntValues = new();
        public Dictionary<string, bool> BoolValues = new();
        public Dictionary<string, string> StringValues = new();
    }
}
#endif
