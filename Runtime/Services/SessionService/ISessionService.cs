using System;

namespace TapEmpire.Services
{
    public interface ISessionService : IService
    {
        TimeSpan GetTotalInactiveTime();
        void ResetTotalInactiveTime();
    }
}