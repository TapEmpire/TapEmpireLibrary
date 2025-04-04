using System;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public static class ExecutionActionExtensions
    {
        public static UniTask<bool> ToUniTask<T>(this IExecutionAction<T> action) where T : Enum
        {
            var completion = new UniTaskCompletionSource<bool>();
            return completion.Task;
        }

        // public static async Task<bool> AsyncWaitForCompletion(this Tween tween)
        // {
        //     TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        //     tween.OnComplete(() => tcs.SetResult(true))
        //          .OnKill(() => tcs.TrySetResult(false));

        //     return await tcs.Task;
        // }

        public static IExecutionAction<T> RunExecute<T>(this IExecutionAction<T> action, T flow) where T : Enum
        {
            action.Execute(flow);
            return action;
        }
    }
}