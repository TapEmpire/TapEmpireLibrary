using Firebase.RemoteConfig;
using R3;

namespace TapEmpire.Services
{
    public interface IFirebaseService : IService
    {
        public ReadOnlyReactiveProperty<bool> IsLoaded { get; }

        public IRemoteConfiguration RemoteConfiguration { get; }

        void Crash();

        FirebaseRemoteConfig GetNativeConfig();
    }
}
