using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/LiveOpsSettings", fileName = "LiveOpsSettings")]
    public class LiveOpsSettings : ScriptableObject
    {
        [SerializeReference] public List<LiveOpsData> LiveOps;
        [field: SerializeField] public List<UpdateAction> ProcessingOrder { get; private set; } = new()
        {
            UpdateAction.Finish,
            UpdateAction.Start,
            UpdateAction.Update,
        };
        public float UpdateDelaySeconds = 1.5f;
    }
}