using System;
using R3;

namespace TapEmpire.Utilities
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
    }
}
