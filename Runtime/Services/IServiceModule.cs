using System;
using Zenject;

namespace TapEmpire.Modules
{
    public interface IServiceModule : IDisposable
    {
    }

    public interface IGenericServiceModule : IDisposable
    {
        void Initialize(DiContainer diContainer);
    }
}