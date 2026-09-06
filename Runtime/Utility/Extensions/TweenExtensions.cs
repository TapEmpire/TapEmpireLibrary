
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;

namespace TapEmpire.Utility
{
    public static class TweenExtensions
    {
        public static UniTask ToUniTask(this Tween tween)
        {
            return tween.AsyncWaitForCompletion().AsUniTask();
        }

        public static IDisposable ToDisposable(this Tween tween)
        {
            return Disposable.Create(() =>
            {
                if (tween != null && tween.IsActive())
                    tween.Kill();
            });
        }

        public static void Forget(this Tween tween)
        {
        }

        public static T JoinTo<T>(this T tween, Sequence sequence) where T : Tween
        {
            sequence.Join(tween);
            return tween;
        }

        public static T AppendTo<T>(this T tween, Sequence sequence) where T : Tween
        {
            sequence.Append(tween);
            return tween;
        }

        public static T InsertTo<T>(this T tween, Sequence sequence, float atPosition) where T : Tween
        {
            sequence.Insert(atPosition, tween);
            return tween;
        }
    }
}