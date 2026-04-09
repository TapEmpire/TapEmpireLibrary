using System.Collections.Generic;
using TapEmpire.Patterns.Strategy;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/LiveOpsSettings", fileName = "LiveOpsSettings")]
    public class LiveOpsSettings : ScriptableObject
    {
        [SerializeReference] public List<LiveOpsData> LiveOps;
        [SerializeReference] public List<IHandler> ConditionHandlers = new();
        [field: SerializeField] public List<UpdateAction> ProcessingOrder { get; private set; } = new()
        {
            UpdateAction.Finish,
            UpdateAction.Start,
            UpdateAction.Update,
        };
    }
}