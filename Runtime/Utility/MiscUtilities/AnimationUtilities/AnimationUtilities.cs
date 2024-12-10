using UnityEngine;
using DG.Tweening;
using System;

namespace TapEmpire.Utility
{
    public static class AnimationUtilities
    {
        public static float PlayFlyAnimationSequenced(Transform origin, Vector3 targetPosition, Quaternion? targetRotation,
            float animationTime, System.Action onComplete = null)
        {
            const float turnAroundTime = 0.2f;
            float duration = 0.0f;

            var targetSameY = new Vector3(targetPosition.x, origin.position.y, targetPosition.z);

            var rotationTimeBefore = MathUtility.GetRotationTime(origin.forward,
                    (targetSameY - origin.position).normalized, turnAroundTime);
            var lookAtBeforeMoveTween = origin.DOLookAt(targetSameY, rotationTimeBefore);
            duration += rotationTimeBefore;

            var moveTween = DoQuadraticMoveInternal(origin, () => targetPosition, animationTime, AnimationConstants.StandardBezierHeight)
                .SetEase(Ease.Linear);
            duration += animationTime;

            var sequence = DOTween.Sequence(targetPosition)
                .Append(lookAtBeforeMoveTween)
                .Append(moveTween);

            if (targetRotation.HasValue)
            {
                var rotationTimeAfter = MathUtility.GetRotationTime(origin.rotation, targetRotation.Value, turnAroundTime);
                var lookAtAfterMoveTween = origin.DORotate(targetRotation.Value.eulerAngles, rotationTimeAfter);
                duration += rotationTimeAfter;

                sequence.Append(lookAtAfterMoveTween);
            }

            sequence.OnComplete(() => onComplete?.Invoke());

            return duration;
        }

        public static void PlayFlyAnimation(Transform target, Transform newParent, float animationTime,
            System.Action onComplete, bool isStatic = false, AnimationTransform animationTransform = null)
        {
            animationTransform ??= AnimationTransform.Default;
            target.DOKill();
            target.SetParent(newParent, true);

            var rotateTween = target.DOLocalRotate(animationTransform.Rotation, animationTime).SetEase(Ease.Linear);
            var scaleTween = target.DOScale(animationTransform.Scale, animationTime).SetEase(Ease.Linear);

            var position = newParent.position;
            System.Func<Vector3> positionAction = isStatic ? () => position : () => newParent.position;

            var moveTween = DoQuadraticMoveInternal(target, positionAction, animationTime, AnimationConstants.StandardBezierHeight)
                .SetEase(Ease.Linear);

            DOTween.Sequence(target)
                .SetUpdate(UpdateType.Late)
                .Join(rotateTween).Join(scaleTween).Join(moveTween)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static void PlayFlyAnimationLocal(Transform target, Transform newParent,
            float animationTime, System.Action onComplete, AnimationTransform animationTransform = null)
        {
            animationTransform ??= AnimationTransform.Default;
            target.DOKill();
            target.SetParent(newParent, true);

            var rotateTween = target.DOLocalRotate(animationTransform.Rotation, animationTime).SetEase(Ease.Linear);
            var scaleTween = target.DOScale(animationTransform.Scale, animationTime).SetEase(Ease.Linear);

            var moveTween = DoLocalQuadraticMove(target, animationTime, AnimationConstants.StandardBezierHeight)
                .SetEase(Ease.Linear);

            DOTween.Sequence(target).Join(rotateTween).Join(scaleTween).Join(moveTween).OnComplete(() => onComplete?.Invoke());
        }

        public static void PlayFlyAnimation(IAnimatable animatable, Transform newParent, float animationTime,
            System.Action onComplete, bool isStatic = false, AnimationTransform animationTransform = null)
        {
            animationTransform ??= AnimationTransform.Default;
            var target = animatable.MainContainer;
            animatable.StopAnimations();
            target.SetParent(newParent, true);

            var rotateTween = target.DOLocalRotate(Vector3.zero, animationTime).SetEase(Ease.Linear);
            var scaleTween = target.DOScale(animationTransform.Scale, animationTime).SetEase(Ease.Linear);

            var position = newParent.position;
            System.Func<Vector3> positionAction = isStatic ? () => position : () => newParent.position;

            var moveTween = DoQuadraticMoveInternal(target, positionAction, animationTime, AnimationConstants.StandardBezierHeight)
                .SetEase(Ease.Linear);

            var sequence = DOTween.Sequence(target)
                .SetUpdate(UpdateType.Late)
                .Join(rotateTween).Join(scaleTween).Join(moveTween);

            if (animatable.HasNonStandardPivot)
            {
                var innerRotateTween = animatable.ProxyContainer.DOLocalRotate(animationTransform.Rotation, animationTime).SetEase(Ease.Linear);
                var positionTween = animatable.MoveCenterToPivot(animationTransform.PivotPosition, animationTime);

                sequence.Join(innerRotateTween).Join(positionTween);
            }

            sequence.OnComplete(() => onComplete?.Invoke());
        }

        public static void PlayFlyAnimationLocal(IAnimatable animatable, Transform newParent,
            float animationTime, System.Action onComplete, AnimationTransform animationTransform = null)
        {
            animatable.StopAnimations();
            PlayFlyAnimationLocal(animatable.MainContainer, newParent, animationTime, onComplete, animationTransform);
        }

        private static Tweener DoLocalQuadraticMove(Transform transform, float animationTime, float height)
        {
            var yShift = Mathf.Max(transform.localPosition.y, 0.0f) + height;

            var start = transform.localPosition;
            var end = Vector3.zero;
            var p0 = MathUtility.yShift(0.5f * (start + end), yShift);
            return DOVirtual
                .Float(0.0f, 1.0f, animationTime, (t) =>
                {
                    transform.localPosition = BazzierUtility.CalculateQuadraticBezierPoint(t, start, p0, end);
                })
                .SetTarget(transform);
        }

        private static Tweener DoQuadraticMoveInternal(Transform transform, Func<Vector3> targetPosition, float animationTime, float height)
        {
            var yShift = Mathf.Max(transform.position.y, targetPosition().y) + height;

            var start = transform.position;
            var end = targetPosition();
            var p0 = MathUtility.yShift(0.5f * (start + end), yShift);
            return DOVirtual
                .Float(0.0f, 1.0f, animationTime, (t) =>
                {
                    if (transform)
                    {
                        transform.position = BazzierUtility.CalculateQuadraticBezierPoint(t, start, p0, targetPosition());
                    }
                })
                .SetTarget(transform);
        }

        #region Simple animations

        public static void PlayAppearAnimation(Transform target, System.Action onComplete = null)
        {
            target.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());
        }

        public static void PlayDisappearAnimation(Transform target, System.Action onComplete = null)
        {
            target.DOScale(0.0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => onComplete?.Invoke());
        }

        public static void SimpleLocalFly(IAnimatable animatable, Transform newParent, float animationTime,
            System.Action onComplete = null)
        {
            var target = animatable.MainContainer;
            animatable.StopAnimations();
            target.SetParent(newParent, true);
            target.DOLocalMove(Vector3.zero, animationTime).SetEase(Ease.InCubic).OnComplete(() => onComplete?.Invoke());
        }

        #endregion Simple animations
    }
}
