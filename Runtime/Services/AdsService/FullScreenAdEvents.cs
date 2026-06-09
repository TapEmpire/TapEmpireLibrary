using System;

namespace TapEmpire.Services
{
    public static class FullScreenAdEvents
    {
        public static event Action OnOpened;
        public static event Action OnClosed;

        internal static void NotifyOpened() => OnOpened?.Invoke();
        internal static void NotifyClosed() => OnClosed?.Invoke();
    }
}
