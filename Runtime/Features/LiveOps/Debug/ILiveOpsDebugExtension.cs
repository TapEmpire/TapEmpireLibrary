using System;

namespace TapEmpire.Services.LiveOps
{
    public interface ILiveOpsDebugExtension : IDisposable
    {
        void Initialize(ILiveOps liveOps);
        void Read();
        void Apply();
    }
}
