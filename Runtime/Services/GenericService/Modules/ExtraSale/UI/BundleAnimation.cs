using System;
using Spine.Unity;
using UnityEngine;
using R3;
using TapEmpire.Feature.Effects;

namespace TapEmpire.Modules
{
    public class BundleAnimation : MonoBehaviour
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private BlickAnimation _blickAnimation;
        [SerializeField] string _idleName = string.Empty;
        [SerializeField] string _activeName = string.Empty;

        private IDisposable _disposable;

        private void Start()
        {
            _disposable = _blickAnimation.OnStart.Subscribe(_ => PlayAnimation());
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            GetAnimationNames();
        }
#endif

        private void GetAnimationNames()
        {
            var state = _skeletonGraphic.AnimationState;
            var data = _skeletonGraphic.Skeleton?.Data;

            var currentEntry = state.GetCurrent(0);
            _idleName = currentEntry?.Animation?.Name;

            var index = data.Animations.FindIndex(animation => animation.Name == _idleName);
            _activeName = data.Animations.Items[index - 1].Name;
        }

        public void PlayAnimation()
        {
            var state = _skeletonGraphic.AnimationState;
            state.SetAnimation(0, _activeName, false);
            state.AddAnimation(0, _idleName, true, 0.0f);
        }
    }
}
