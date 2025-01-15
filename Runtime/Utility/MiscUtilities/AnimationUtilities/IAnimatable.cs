using System;
using DG.Tweening;
using UnityEngine;
using R3;

namespace TapEmpire.Utility
{
    public interface IAnimatable
    {
        public IObservable<Unit> AnimationsStarted { get; }
        public IObservable<Unit> AnimationsStopped { get; }
        
        public Transform MainContainer { get; }
        public Transform ProxyContainer { get; }

        public void StopAnimations();

        public bool HasNonStandardPivot { get; }
        public Tween MoveCenterToPivot(PivotPosition pivotPosition, float animationTime);
    }
}
