using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace TapEmpire.Modules
{
    public interface IServiceModule : IDisposable
    {
    }

    public interface IGenericServiceModule : IDisposable
    {
        UniTask InitializeAsync(DiContainer diContainer, CancellationToken cancellationToken);
    }
}