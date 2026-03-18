using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services.Shop;
using TapEmpire.UI;
using Zenject;
using TapEmpire.Services.LiveOps;

namespace TapEmpire.LiveOps.UI
{
    public class LiveOpsViewModel : IUIViewModel
    {
        public ILiveOps LiveOps { get; private set; }

        public T As<T>() where T : ILiveOps => (T)LiveOps;

        public LiveOpsViewModel(ILiveOps liveOps)
        {
            LiveOps = liveOps;
        }
    }
}