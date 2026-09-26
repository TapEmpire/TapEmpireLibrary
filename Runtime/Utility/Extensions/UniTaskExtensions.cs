using System;
using Cysharp.Threading.Tasks;

namespace TapEmpire.Utility
{
    public static class UniTaskExtensions
    {
        public static async UniTask Finally(this UniTask task, Action finallyAction)
        {
            try
            {
                await task;
            }
            finally
            {
                finallyAction();
            }
        }

        public static async UniTask<T> Finally<T>(this UniTask<T> task, Action finallyAction)
        {
            try
            {
                return await task;
            }
            finally
            {
                finallyAction();
            }
        }
    }
}
