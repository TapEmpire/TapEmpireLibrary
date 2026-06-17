using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace TapEmpire.Utility
{
    public static class R3Utilities
    {
        public static Observable<Unit> ConvertToObservable(Action<Action> methodWithCallback)
        {
            return Observable.Create<Unit>(observer =>
            {
                methodWithCallback(() =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                });

                return Disposable.Empty;
            });
        }

        public static IDisposable OnceTrue(this Observable<bool> source, Action onNext)
            => source.Where(v => v).Take(1).Subscribe(_ => onNext());

        public static async UniTask WaitTrue(this Observable<bool> source, CancellationToken cancellationToken = default)
            => await source.Where(v => v).FirstAsync(cancellationToken: cancellationToken);

        public static IDisposable OnceWhen<T>(this Observable<T> source, Func<T, bool> predicate, Action<T> onNext)
            => source.Where(predicate).Take(1).Subscribe(onNext);

        public static async UniTask<T> WaitWhen<T>(this Observable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
            => await source.Where(predicate).FirstAsync(cancellationToken: cancellationToken);
    }
}
