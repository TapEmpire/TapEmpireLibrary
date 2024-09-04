using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Services;

namespace TapEmpire.UI
{
    public interface IUIService : IService
    {
        bool TryGetModel<T>(out T model)
            where T : IUIViewModel;

        bool TryGetView(IUIViewModel model, out UIView view);

        UniTask OpenViewAsync(UIView viewPrefab, IUIViewModel viewModel, CancellationToken cancellationToken,
            bool tryUseDefaultFadeIn = true, bool openAsPopup = false);

        UniTask CloseViewAsync(IUIViewModel viewModel, CancellationToken cancellationToken,
            bool tryUseDefaultFadeOut = true);

        // ивенты в тестовом режиме
        event Action<IUIViewModel> OnBeforeOpenView;

        event Action<IUIViewModel> OnAfterOpenView; 

        event Action<IUIViewModel> OnBeforeCloseView;
        
        event Action<IUIViewModel> OnAfterCloseView;

        void AddToContext(string name, RectTransform transform);
        void RemoveFromContext(string name);

        // Should be ReactiveDictionary, but R3 has none.
        Dictionary<string, RectTransform> ShibariContext { get; }
    }
}