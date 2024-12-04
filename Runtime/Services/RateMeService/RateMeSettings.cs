using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/RateMeSettings", fileName = "RateMeSettings")]
    public class RateMeSettings : ScriptableObject
    {
        [SerializeField]
        public bool Enable;

        [SerializeField]
        public List<int> Levels;

        public bool IsRateMeNeeded(int numberLevel)
        {
            return Enable ? Levels.Contains(numberLevel) : false;
        }
    }
}
