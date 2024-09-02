namespace TapEmpire.Services
{
    public interface IRemoteConfiguration
    {
        public bool TryGetFloat(string key, out float value);
        public bool TryGetString(string key, out string value);
        public bool TryGetBool(string key, out bool value);
        public bool TryGetInt(string key, out int value);

        public string GetString(string key, string defaultValue);
        public int GetInt(string key, int defaultValue);
    }
}
