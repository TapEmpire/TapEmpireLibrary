using System;
using LunarConsolePlugin;
using TapEmpire.Services;
using UnityEngine;
using Zenject;

namespace TapEmpire.Game
{
    [Serializable]
    public class LunarConsoleStartAction : IStartAction
    {
        [Inject]
        private ISystemService _systemService;

        [SerializeField]
        private LunarConsole _prefab;

        public void Apply()
        {
            if (_systemService.StaticSettings.Debug && _prefab != null)
            {
                UnityEngine.Object.Instantiate(_prefab);
            }
        }
    }
}
