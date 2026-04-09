#if TEL_CLOUD_SAVE
namespace TapEmpire.Services
{
    public readonly struct CloudSaveOperationResult
    {
        public bool Success { get; }
        public bool Skipped { get; }
        public string Message { get; }

        public CloudSaveOperationResult(bool success, bool skipped, string message)
        {
            Success = success;
            Skipped = skipped;
            Message = message;
        }

        public static CloudSaveOperationResult Completed(string message = "")
            => new(true, false, message);

        public static CloudSaveOperationResult Failed(string message)
            => new(false, false, message);

        public static CloudSaveOperationResult Ignored(string message)
            => new(false, true, message);
    }
}
#endif
