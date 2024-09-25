using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.Utility
{
    public static class DoTweenUtility
    {
        public static Sequence ExplodeBounce(Transform transform, DoTweenExplodeBounceData data)
        {
            var sequence = DOTween.Sequence();
            var currentScaleIncrease = data.InitialScaleIncrease;
            var duration = data.Duration;
            for (var i = 0; i < data.BouncesCount; i++)
            {
                var targetScale = 1 + ((currentScaleIncrease - 1) / Mathf.Pow(2, i));
                sequence.Append(transform.DOScale(targetScale, duration).SetEase(data.Ease));
                sequence.Append(transform.DOScale(1, duration).SetEase(data.Ease)); 
                duration *= data.IterationDecreaseMultiplier;
            }
            sequence.Play();
            return sequence;
        }
        
        public static Tween Fade(CanvasGroup canvasGroup, float targetFade, float duration = 0.5f, Ease ease = Ease.Linear)
        {
            return canvasGroup.DOFade(targetFade, duration).SetEase(ease);
        }
    }
}