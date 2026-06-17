using LunarConsolePlugin;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpireLibrary.Utility
{
    public class LunarConsoleOpenHandler : MonoBehaviour
    {
        [Inject] private IAdsService _adsService;

        private bool _bannerWasShown;

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
            _bannerWasShown = _adsService.ShowBanner(false);
        }

        public void OnConsoleClosed()
        {
            _adsService.ShowBanner(_bannerWasShown);
        }
    }
}