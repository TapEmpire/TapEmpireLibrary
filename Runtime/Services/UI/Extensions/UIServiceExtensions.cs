using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TapEmpire.UI
{
    public static class UIServiceExtensions
    {
        public static UniTask TryCloseViewAsync<T>(this IUIService self, CancellationToken cancellationToken = default, bool tryUseDefaultFadeOut = true, bool log = false)
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

        public static async UniTask OpenViewAwaitable<T>(this IUIService self, UIView viewPrefab, T viewModel, CancellationToken cancellationToken, bool tryUseFade = true, bool asPopup = false)
            where T : IUIViewModel
        {
            var tcs = new UniTaskCompletionSource();

            void OnClose(IUIViewModel closedModel)
            {
                if (!ReferenceEquals(closedModel, viewModel)) return;
                self.OnAfterCloseView -= OnClose;
                tcs.TrySetResult();
            }

            self.OnAfterCloseView += OnClose;
            try
            {
                await self.OpenViewAsync(viewPrefab, viewModel, cancellationToken, tryUseFade, asPopup);
                await tcs.Task;
            }
            finally
            {
                self.OnAfterCloseView -= OnClose;
            }
        }
    }
}