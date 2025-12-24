using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Settings;

namespace TapEmpire.Services
{
    public interface ISystemService : IService
    {
        Subject<bool> OnApplicationFocusChanged { get; }
        Subject<Unit> OnSessionStarted { get; }

        GameStartSettings StaticSettings { get; }
        SystemSettings SystemSettings { get; }

        bool CanPlayOffline { get; }
    }
}