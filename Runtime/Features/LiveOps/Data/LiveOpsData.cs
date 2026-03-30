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
        public State State = State.NotStarted;
        public int Inner = 0;
        public int Value = 0;
        public int Addend = 0;
    }
}