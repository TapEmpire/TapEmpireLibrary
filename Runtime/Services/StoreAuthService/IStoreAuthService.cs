namespace TapEmpire.Services
{
    public interface IStoreAuthService : IService
    {
        void Login();
        void Logout();
    }
}