using System;
using System.Collections.Generic;
using TapEmpire.Patterns.Strategy;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/LiveOpsSettings", fileName = "LiveOpsSettings")]
    public class LiveOpsSettings : ScriptableObject
    {
        [SerializeReference] public List<LiveOpsData> LiveOps;
        [SerializeReference] public List<IHandler> ConditionHandlers = new();
    }
}