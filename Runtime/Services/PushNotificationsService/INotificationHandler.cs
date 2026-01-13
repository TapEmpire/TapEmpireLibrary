using System;
using Zenject;

namespace TapEmpire.Services.Notifications
{
    public interface INotificationHandler : IDisposable
    {
        public void Initialize(DiContainer diContainer);
    }
}