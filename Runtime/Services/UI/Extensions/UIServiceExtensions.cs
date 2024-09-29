using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.UI
{
    public static class UIServiceExtensions
    {
        public static UniTask TryCloseViewAsync<T>(this IUIService self, CancellationToken cancellationToken, bool tryUseDefaultFadeOut = true, bool log = false)
            where T : IUIViewModel
        {
            if (self.TryGetModel<T>(out var model))
            {
                return self.CloseViewAsync(model, cancellationToken, tryUseDefaultFadeOut);
            }
            if (log)
            {
                Debug.Log($"No opened view with model of type {typeof(T)}");
            }
            return UniTask.CompletedTask;
        }
    }
}