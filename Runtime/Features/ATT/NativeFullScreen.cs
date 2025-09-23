using System.Runtime.InteropServices;

namespace TapEmpire.Features.ATT
{
    public static class NativeFullScreen
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void nfs_show(string title, string message);
        [DllImport("__Internal")] private static extern void nfs_dismiss();
#else
        private static void nfs_show(string t, string m) => UnityEngine.Debug.Log($"[NFS] Show: {t} - {m}");
        private static void nfs_dismiss() => UnityEngine.Debug.Log("[NFS] Dismiss");
#endif

        public static void Show(string title, string message) => nfs_show(title, message);
        public static void Dismiss() => nfs_dismiss();
    }
}
