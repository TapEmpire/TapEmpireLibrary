using UnityEngine;
using DG.Tweening;
using System;

namespace TapEmpire.Utility
{
    public static class AnimationUtilities
    {
        public static Tweener PlayBezierAnimation(Transform target, Vector3 end, Vector3 p0, float animationTime)
        {
            var start = target.position;
            return DOVirtual
                .Float(0.0f, 1.0f, animationTime, (t) =>
                {
                    if (target != null)
                    {
                        target.position = BazzierUtility.CalculateQuadraticBezierPoint(t, start, p0, end);
                    }
                })
                .SetTarget(target);
        }

        public static Tweener PlayBezierAnimation(Transform target, Vector3 end, float height, float animationTime,
            System.Action onComplete)
        {
            target.DOKill();

            System.Func<Vector3> positionAction = () => end;

            return DoQuadraticMoveInternal(target, positionAction, animationTime, height)
                .SetEase(Ease.Linear)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static Sequence PlayHalfBezierAnimation(Transform target, Vector3 end, float height, float animationTime,
            System.Action onHalf)
        {
            var moveTween = PlayBezierAnimation(target, end, height, animationTime, null);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(moveTween);
            sequence.InsertCallback(moveTween.Duration() * 0.5f, () =>
            {
                onHalf?.Invoke();
                sequence.Kill();
            });
            sequence.SetLink(target.gameObject);
            sequence.SetTarget(target);

            return sequence;
        }

        public static Sequence PlayHalfParabolisticAnimation(Transform target, Vector3 end, float height, float animationTime,
            System.Action onHalf)
        {
            target.DOKill();

            var moveTween = DoParabolisticMoveInternal(target, end, animationTime, height)
                .SetEase(Ease.Linear);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(moveTween);
            sequence.InsertCallback(moveTween.Duration() * 0.5f, () =>
            {
                onHalf?.Invoke();
                sequence.Kill();
            });
            sequence.SetLink(target.gameObject);
            sequence.SetTarget(target);

            return sequence;
        }

        #region Parabolistic

        private static Tweener DoParabolisticMoveInternal(Transform transform, Vector3 end, float animationTime, float height)
        {
            var yShift = Mathf.Max(transform.position.y, end.y) + height;
            var start = transform.position;

            return DOVirtual
                .Float(0.0f, 1.0f, animationTime, (t) =>
                {
                    if (transform)
                    {
                        transform.position = ParabolisticUtility.CalculateSimpleParabolisticPoint(start, end, height, t);
                    }
                })
                .SetTarget(transform);
        }

        #endregion Parabolistic

        #region Bezier

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

        #endregion Bezier
    }
}
