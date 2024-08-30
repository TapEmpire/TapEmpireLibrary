namespace TapEmpire.Services
{
    public interface ISerializationService : IService
    {
        string GetSerializedConfig(string configName);
    }
}
