#if TEL_CLOUD_SAVE
namespace TapEmpire.Services
{
    public readonly struct CloudSaveLoadResult
    {
        public bool Success { get; }
        public string Message { get; }
        public ProgressSnapshot Snapshot { get; }
        public bool HasSnapshot => Snapshot != null;

        public CloudSaveLoadResult(bool success, string message, ProgressSnapshot snapshot)
        {
            Success = success;
            Message = message;
            Snapshot = snapshot;
        }

        public static CloudSaveLoadResult Completed(ProgressSnapshot snapshot)
            => new(true, string.Empty, snapshot);

        public static CloudSaveLoadResult Failed(string message)
            => new(false, message, null);
    }
}
#endif
