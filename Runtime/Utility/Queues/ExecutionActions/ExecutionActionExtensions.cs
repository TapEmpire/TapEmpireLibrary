using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public static class ExecutionActionExtensions
    {
        public static UniTask<bool> ToUniTask(this IExecutionAction action)
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

        public static IExecutionAction RunExecute(this IExecutionAction action)
        {
            action.Execute();
            return action;
        }
    }
}