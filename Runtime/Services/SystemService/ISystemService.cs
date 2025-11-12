using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Services
{
    public interface ISystemService : IService
    {
        Subject<bool> OnApplicationFocusChanged { get; }
        Subject<Unit> OnSessionStarted { get; }
    }
}