using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/LiveOpsSettings", fileName = "LiveOpsSettings")]
    public class LiveOpsSettings : ScriptableObject
    {
        [SerializeReference] public List<LiveOpsData> LiveOps;
        public float UpdateDelaySeconds = 1.5f;
        public float UpdateVisualIntervalSeconds = 0.1f;
    }
}