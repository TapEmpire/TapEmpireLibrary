using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Services;

namespace TapEmpire.UI
{
    public partial interface IUIService : IService
    {
        bool TryGetModel<T>(out T model)
            where T : IUIViewModel;

        // TODO в первом пазле было как костыль, я бы убрал, т.к. оп идее вьюху наружу не даем - взаимодействие идет через model
        bool TryGetView(IUIViewModel model, out UIView view);

        UniTask OpenViewAsync<T>(UIView viewPrefab, T viewModel, CancellationToken cancellationToken, bool tryUseFade = true, bool asPopup = false) where T : IUIViewModel;

        UniTask CloseViewAsync(IUIViewModel viewModel, CancellationToken cancellationToken, bool tryUseFade = true);

        UniTask CloseAllViewsExcept<T>(CancellationToken cancellationToken, bool tryUseFade = true) where T : IUIViewModel;

        // TODO ивенты убрал бы эти
        event Action<IUIViewModel> OnBeforeOpenView;

        event Action<IUIViewModel> OnAfterOpenView; 

        event Action<IUIViewModel> OnBeforeCloseView;
        
        event Action<IUIViewModel> OnAfterCloseView;

        // TODO ShibariContext тоже убрал бы из общей реализации, мне кажется не везде будет востребован, я бы его сбоку реализовал
        void AddToContext(string name, RectTransform transform);
        void RemoveFromContext(string name);

        // Should be ReactiveDictionary, but R3 has none.
        Dictionary<string, RectTransform> ShibariContext { get; }
        
        IUILocker UILocker { get; }

        void SetViewsCanvasesInteractionState(bool state);
    }
}