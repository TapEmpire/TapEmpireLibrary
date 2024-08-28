using Firebase.RemoteConfig;
using TapEmpire.Services;

namespace TapEmpire.Services
{
    public interface IFirebaseService : IService
    {
        void Crash();

        FirebaseRemoteConfig GetConfig();
    }
}
