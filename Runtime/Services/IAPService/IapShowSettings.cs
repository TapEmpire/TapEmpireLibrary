using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/IapShowSettings", fileName = "IapShowSettings")]
    public class IapShowSettings : ScriptableObject
    {
        [SerializeField]
        public bool Enable;

        [SerializeField]
        public List<int> Levels;

        public bool ShouldShowIapOffer(int numberLevel)
        {
            return Enable ? Levels.Contains(numberLevel) : false;
        }
    }
}