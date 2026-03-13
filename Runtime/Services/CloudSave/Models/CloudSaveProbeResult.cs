namespace TapEmpire.Services
{
    public readonly struct CloudSaveProbeResult
    {
        public bool ProviderAvailable { get; }
        public bool HasCloudData { get; }
        public long CloudDataTimestampMs { get; }
        public string Message { get; }
        public ProgressSnapshot CloudSnapshot { get; }

        public CloudSaveProbeResult(bool providerAvailable, bool hasCloudData, long cloudDataTimestampMs, string message, ProgressSnapshot cloudSnapshot)
        {
            ProviderAvailable = providerAvailable;
            HasCloudData = hasCloudData;
            CloudDataTimestampMs = cloudDataTimestampMs;
            Message = message;
            CloudSnapshot = cloudSnapshot;
        }

        public static CloudSaveProbeResult NoProvider(string message)
            => new(false, false, 0, message, null);

        public static CloudSaveProbeResult NoData()
            => new(true, false, 0, string.Empty, null);

        public static CloudSaveProbeResult DataFound(long timestampMs, ProgressSnapshot cloudSnapshot)
            => new(true, true, timestampMs, string.Empty, cloudSnapshot);

        public static CloudSaveProbeResult Failed(string message)
            => new(false, false, 0, message, null);
    }
}
