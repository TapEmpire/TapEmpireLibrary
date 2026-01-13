using System.Collections.Generic;
using DG.Tweening;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Fragments
{
    public static class AnimationFragment
    {
        public static Sequence CollectResource(Transform prefab, Transform parent, 
            int amount, Vector3 start, Vector3 end, float radius, float extent, System.Action callback)
        {
            var points = GetRadialSpreadPoints(start, amount, radius, extent);
            var animation = DOTween.Sequence();

            foreach (var point in points)
            {
                var resource = GameObject.Instantiate(prefab, start, Quaternion.identity, parent);

                var sequence = DOTween.Sequence();
                resource.DOMove(point, 0.3f).AppendTo(sequence);
                resource.DOMove(end, 0.5f).SetDelay(Random.Range(0.05f, 0.2f)).SetEase(Ease.InBack).AppendTo(sequence);
                sequence.AppendCallback(() => {
                    callback?.Invoke();
                    GameObject.Destroy(resource.gameObject);
                });
                animation.Join(sequence);
            }

            animation.SetLink(parent.gameObject);

            return animation;
        }

        public static List<Vector2> GetRadialSpreadPoints(Vector3 center, int count, float radius, float extent)
        {
            List<Vector2> points = new List<Vector2>(count);

            float phase = Random.Range(0f, 360.0f);

            for (int i = 0; i < count; i++)
            {
                float stepAngle = 360.0f / count;
                float angle = phase + i * stepAngle;
                float finalRadius = GetScatterRadius(radius, extent);

                float rad = angle * Mathf.Deg2Rad;

                var offset = new Vector3(finalRadius * Mathf.Cos(rad), finalRadius * Mathf.Sin(rad), 0.0f);
                points.Add(center + offset);
            }

            return points;
        }

        private static float GetScatterRadius(float radius, float extent)
        {
            return radius + Random.Range(-extent, extent);
        }
    }
}