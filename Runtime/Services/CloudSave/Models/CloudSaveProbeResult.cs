namespace TapEmpire.Services
{
    public readonly struct CloudSaveProbeResult
    {
        public bool ProviderAvailable { get; }
        public bool HasCloudData { get; }
        public long CloudDataTimestampMs { get; }
        public string Message { get; }

        public CloudSaveProbeResult(bool providerAvailable, bool hasCloudData, long cloudDataTimestampMs, string message)
        {
            ProviderAvailable = providerAvailable;
            HasCloudData = hasCloudData;
            CloudDataTimestampMs = cloudDataTimestampMs;
            Message = message;
        }

        public static CloudSaveProbeResult NoProvider(string message)
            => new(false, false, 0, message);

        public static CloudSaveProbeResult NoData()
            => new(true, false, 0, string.Empty);

        public static CloudSaveProbeResult DataFound(long timestampMs)
            => new(true, true, timestampMs, string.Empty);

        public static CloudSaveProbeResult Failed(string message)
            => new(false, false, 0, message);
    }
}
