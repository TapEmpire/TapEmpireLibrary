using R3;
using UnityEngine;

namespace TapEmpire.Utility
{
    /// <summary>
    /// Used to register R3 subscription exceptions
    /// </summary>
    public static class R3UnityProviderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void SetDefaultObservableSystem()
        {
            ObservableSystem.RegisterUnhandledExceptionHandler(static ex => UnityEngine.Debug.LogException(ex));
        }
    }
}