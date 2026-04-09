#if TEL_CLOUD_SAVE
namespace TapEmpire.Services
{
    public interface ICloudSaveSerializer<T>
    {
        int ExcludedKeysCount { get; }
        T Export();
        void Import(T snapshot);
    }
}
#endif
