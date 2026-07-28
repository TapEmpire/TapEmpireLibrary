using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TapEmpire.Settings
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/GameStartSettings", fileName = "GameStartSettings")]
    public class GameStartSettings : ScriptableObject
    {
        public readonly Subject<GameStartSettings> OnDataChanged = new ();

        public bool Debug;

        [ShowIf(nameof(Debug))]
        public bool AutoRestartLevel;

        [ShowIf(nameof(Debug))]
        public bool SkipInters;

        public bool IgnoreConnection = false;

        public int FrameRate = 60;

        public void BroadcastUpdate() => OnDataChanged.OnNext(this);
    }
}
