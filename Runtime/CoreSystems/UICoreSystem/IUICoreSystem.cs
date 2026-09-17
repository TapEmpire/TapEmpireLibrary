using System;
using R3;

namespace TapEmpire.CoreSystems
{
    public interface IUICoreSystem : ICoreSystem
    {
        ReadOnlyReactiveProperty<bool> IsUIBlocked { get; }

        void BlockUI(bool shouldBlock);

        IDisposable BlockUI();
    }
}
