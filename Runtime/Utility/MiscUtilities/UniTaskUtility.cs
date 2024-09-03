using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public static class UniTaskUtility
    {
        public static async UniTask ExecuteAfterFrames(int frames, Action callback, CancellationToken cancellationToken)
        {
            if (frames > 0)
            {
                await UniTask.DelayFrame(frames, cancellationToken: cancellationToken);
                callback.Invoke();
            }
            else
            {
                callback.Invoke();
            }
        }
        
        public static async UniTask ExecuteNextFrame(Action callback, CancellationToken cancellationToken)
        {
            await UniTask.Yield(cancellationToken: cancellationToken);
            callback.Invoke();
        }
        
        
        public static async UniTask ExecuteAfterSeconds(float seconds, Action callback, CancellationToken cancellationToken)
        {
            if (seconds > 0)
            {
                await UniTask.WaitForSeconds(seconds, cancellationToken:cancellationToken);
                callback.Invoke();
            }
            else
            {
                callback.Invoke();
            }
        }

        public static async UniTask SafeExecuteAsync(UniTask uniTask, CancellationToken cancellationToken, bool logCancel = false)
        {
            try
            {
                await uniTask.AttachExternalCancellation(cancellationToken);
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                if (logCancel)
                {
                    UnityEngine.Debug.Log("Token cancelled");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public static async UniTask ExecuteAfter(UniTask uniTask, Action callback)
        {
            await uniTask;
            callback.Invoke();
        }
        
        public static UniTask ToUniTask(this Action action)
        {
            var tcs = new UniTaskCompletionSource();

            try
            {
                action();
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public static async UniTask WhenAll(params Action[] actions)
        {
            var tasks = new UniTask[actions.Length];

            for (int i = 0; i < actions.Length; i++)
            {
                tasks[i] = actions[i].ToUniTask();
            }

            await UniTask.WhenAll(tasks);
        }
    }
}