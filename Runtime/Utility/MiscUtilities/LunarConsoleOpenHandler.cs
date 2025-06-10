using LunarConsolePlugin;
using UnityEngine;

namespace TapEmpireLibrary.Utility
{
    public class LunarConsoleOpenHandler : MonoBehaviour
    {
        public void OnEnable()
        {
            LunarConsole.onConsoleOpened += OnConsoleOpened;
            LunarConsole.onConsoleClosed += OnConsoleClosed;
        }
    
        public void OnDisable()
        {
            LunarConsole.onConsoleOpened -= OnConsoleOpened;
            LunarConsole.onConsoleClosed -= OnConsoleClosed;
        }
    
        public void OnConsoleOpened()
        {
            AdsManager.Instance.HideAllBanners();
        }
    
        public void OnConsoleClosed()
        {
            AdsManager.Instance.ResumeAllBanners();
        }
    }
}