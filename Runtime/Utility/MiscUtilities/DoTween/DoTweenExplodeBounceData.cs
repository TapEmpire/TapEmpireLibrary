using System;
using DG.Tweening;

namespace TapEmpire.Utility
{
    [Serializable]
    public class DoTweenExplodeBounceData
    {
        public float InitialScaleIncrease = 1.2f;
        public float Duration = 0.5f;
        public int BouncesCount = 3;
        public float IterationDecreaseMultiplier = 0.8f;
        public Ease Ease = Ease.OutQuad;
    }
}