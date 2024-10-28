using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using UnityEngine;
using TapEmpire.Services;

namespace TapEmpire.UI
{
    public partial interface IUIService : IService
    {
        bool TryGetModel<T>(out T model) where T : IUIViewModel;

        // TODO в первом пазле было как костыль, я бы убрал, т.к. оп идее вьюху наружу не даем - взаимодействие идет через model
        bool TryGetView(IUIViewModel model, out UIView view);

        UniTask OpenViewAsync<T>(UIView viewPrefab, T viewModel, CancellationToken cancellationToken, bool tryUseFade = true, bool asPopup = false) where T : IUIViewModel;

        UniTask CloseViewAsync(IUIViewModel viewModel, CancellationToken cancellationToken, bool tryUseFade = true);

        UniTask CloseAllViewsExcept<T>(CancellationToken cancellationToken, bool tryUseFade = true) where T : IUIViewModel;

        // TODO ShibariContext тоже убрал бы из общей реализации, мне кажется не везде будет востребован, я бы его сбоку реализовал
        void AddToContext(string name, RectTransform transform);
        void RemoveFromContext(string name);

        // Should be ReactiveDictionary, but R3 has none.
        public ObservableDictionary<string, RectTransform> ShibariContext { get; }
        
        IUILocker UILocker { get; }

        void SetViewsCanvasesInteractionState(bool state);
        
        // TODO я бы все ивент отсюда перенес на messages, но для этого нужно MessagesUtility переносить в либу
        // POPUP
        event Action<IUIViewModel> OnOpenPopup;
        
        event Action<IUIViewModel> OnClosePopup;
        
        IUIViewModel CurrentPopup { get; }
        
        // TODO ивенты убрал бы эти
        event Action<IUIViewModel> OnBeforeOpenView;

        event Action<IUIViewModel> OnAfterOpenView; 

        event Action<IUIViewModel> OnBeforeCloseView;
        
        event Action<IUIViewModel> OnAfterCloseView;
    }
}