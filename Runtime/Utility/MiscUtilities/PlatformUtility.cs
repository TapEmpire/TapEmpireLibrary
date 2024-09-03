using UnityEngine;

namespace TapEmpire.Utility
{
    public static class PlatformUtility
    {
        public static bool IsMobileDevice => Application.platform == RuntimePlatform.Android ||
                                             Application.platform == RuntimePlatform.IPhonePlayer;

        public static bool IsEditorDeviceSimulator => Application.isEditor && !UnityEngine.Device.Application.isEditor;

        public static bool IsListeningTouchInput => IsMobileDevice || IsEditorDeviceSimulator;
    }
}