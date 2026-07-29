using R3;
using UnityEngine;

namespace TapEmpire.Settings
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameStartSettings", fileName = "GameStartSettings")]
    public class GameStartSettings : ScriptableObject
    {
        public readonly Subject<GameStartSettings> OnDataChanged = new ();

        public bool AutoRestartLevel;

        public void BroadcastUpdate() => OnDataChanged.OnNext(this);
    }
}
