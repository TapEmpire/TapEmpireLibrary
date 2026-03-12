using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services.LiveOps
{
    [Serializable]
    public abstract class LiveOpsData
    {
        [SerializeReference] public List<ICondition> Conditions = new();

        // createMethod
        // public OfferRuntimeData ToRuntime(Rarity rarity) => new OfferRuntimeData(this, rarity);

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