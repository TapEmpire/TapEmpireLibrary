namespace TapEmpire.Utility
{
    public static class PlatformInfo
    {
#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern bool te_isSandboxReceipt();
#endif

        public static bool IsTestFlightOrSandboxReceipt()
        {
#if UNITY_IOS && !UNITY_EDITOR
        // True for TestFlight and dev builds that use StoreKit sandbox
        return te_isSandboxReceipt();
#else
            return false;
#endif
        }
    }
}
