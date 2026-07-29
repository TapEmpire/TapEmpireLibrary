using UnityEngine;

namespace TapEmpire.Settings
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameSettings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public bool AutoRestartLevel;
    }
}
