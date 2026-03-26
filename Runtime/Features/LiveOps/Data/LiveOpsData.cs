using System;
using System.Collections.Generic;
using TapEmpire.LiveOps.UI;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public abstract class LiveOpsData
    {
        [SerializeReference] public List<ICondition> Conditions = new();
        public LiveOpsIcon IconPrefab;
        public LiveOpsView LiveOpsPrefab;

        public abstract ILiveOps Create();
    }

    public class StateData
    {
        public State State;
        public int Inner;
        public int Value;
        public int Addend;
    }
}