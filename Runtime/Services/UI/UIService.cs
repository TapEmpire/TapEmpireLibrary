using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TapEmpire.Services;
using TapEmpire.Utility;
using Zenject;
using Object = UnityEngine.Object;
using System.Linq;
using DG.Tweening;

namespace TapEmpire.UI
{
    [Serializable]
    public class UIService : Initializable, IUIService
    {
        [SerializeField]
        private GameObject _canvasPrefab;

        [Header("Fade")]
        [SerializeField]
        private float _fadeDuration = 0.5f;

        [SerializeField]
        private Ease _fadeEase;
        
        [NonSerialized]
        private RectTransform _canvasRectTransform;
        
        [NonSerialized]
        private Dictionary<IUIViewModel, UIView> _views = new();

        private ISceneContextsService _sceneContextsService;
        
        private IUIViewModel _currentPopupModel;
        private DiContainer _coreDiContainer;

        public Dictionary<string, RectTransform> ShibariContext { get; private set; } = new();

        [Inject]
        private void Construct(ISceneContextsService sceneContextsService)
        {
            _sceneContextsService = sceneContextsService;
        }

        public void SetViewsCanvasesInteractionState(bool state)
        {
            // полукостылем пока сделаю, по идее нужны новые интерфейсы
            foreach (var (model, view) in _views)
            {
                if (view is IFadeAbleView fadeAbleView)
                {
                    fadeAbleView.CanvasGroup.interactable = state;
                }
            }
        }

        #region Initializable

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            var canvas = Object.Instantiate(_canvasPrefab);
            Object.DontDestroyOnLoad(canvas);
            _canvasRectTransform = canvas.GetComponent<RectTransform>();
            _sceneContextsService.OnSceneContextInstalled += SceneContextsService_OnSceneContextInstalled;
            return UniTask.CompletedTask;
        }

        private void SceneContextsService_OnSceneContextInstalled(string contextId, SceneContext context)
        {
            if (contextId != "Core")
            {
                return;
            }
            _coreDiContainer = context.Container;
        }

        protected override void OnRelease()
        {
            _views.Clear();
            if (_sceneContextsService != null)
            {
                _sceneContextsService.OnSceneContextInstalled -= SceneContextsService_OnSceneContextInstalled;
            }
        }

        #endregion

        public bool TryGetModel<T>(out T model)
            where T : IUIViewModel
        {
            if (_views.TryGetFirst(keyValue => keyValue.Key is T, out var keyValuePair))
            {
                model = (T)keyValuePair.Key;
                return true;
            }
            model = default;
            return false;
        }

        public bool TryGetView(IUIViewModel model, out UIView view)
        {
            if (_views.TryGetFirst(keyValue => keyValue.Key == model, out var keyValuePair))
            {
                view = keyValuePair.Value;
                return true;
            }
            view = default;
            return false;
        }

        public async UniTask OpenViewAsync<T>(UIView viewPrefab, T viewModel, CancellationToken cancellationToken, bool tryUseFade = true, bool asPopup = false) where T : IUIViewModel
        {
            if (viewPrefab == null)
            {
                Debug.LogError("Trying to open null view");
                return;
            }
            if (_views.ContainsKey(viewModel))
            {
                Debug.Log("View already opened");
                return;
            }
            if (_views.Any(kvp => kvp.Key is T))
            {
                return;
            }
            OnBeforeOpenView?.Invoke(viewModel);
            if (asPopup && _currentPopupModel != null)
            {
                CloseViewAsync(_currentPopupModel, cancellationToken, false).Forget();
                _currentPopupModel = null;
            }
            var view = Object.Instantiate(viewPrefab, _canvasRectTransform);
            view.Model = viewModel;
            _views.Add(viewModel, view);
            if ( _coreDiContainer != null)
            {
                if (view is IInjectable)
                {
                    _coreDiContainer.Inject(view);
                }
                if (viewModel is IInjectable)
                {
                    _coreDiContainer.Inject(viewModel);
                }
            }
            await TryExecuteWithFadeAsync(view.OpenAsync(cancellationToken), view, true, cancellationToken, tryUseFade);
            
            if (asPopup)
            {
                _currentPopupModel = viewModel;
            }
            OnAfterOpenView?.Invoke(viewModel);
        }

        private async UniTask TryExecuteWithFadeAsync(UniTask task, UIView view, bool fadeIn, CancellationToken cancellationToken, bool tryUseFade = true)
        {
            if (tryUseFade && view is IFadeAbleView fadeAbleView)
            {
                fadeAbleView.CanvasGroup.alpha = fadeIn ? 0 : 1;
                DoTweenUtility.Fade(fadeAbleView.CanvasGroup, fadeIn ? 1 : 0, _fadeDuration, _fadeEase);

                using (ListScope<UniTask>.Create(out var tasks))
                {
                    tasks.Add(UniTask.WaitForSeconds(_fadeDuration, cancellationToken:cancellationToken));

                    tasks.Add(task);

                    await UniTask.WhenAll(tasks);
                }
            }
            else
            {
                await task;
            }
        }

        public async UniTask CloseViewAsync(IUIViewModel viewModel, CancellationToken cancellationToken, bool tryUseFade = true)
        {
            if (!_views.TryGetValue(viewModel, out var view))
            {
                return;
            }
            OnBeforeCloseView?.Invoke(viewModel);

            await TryExecuteWithFadeAsync(view.CloseAsync(cancellationToken), view, false, cancellationToken, tryUseFade);
            
            if (_currentPopupModel != null && _currentPopupModel == viewModel)
            {
                _currentPopupModel = null;
            }
            try
            {
                _views.Remove(viewModel);
                Object.Destroy(view.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error destroying view {e.Message}");
            }
            OnAfterCloseView?.Invoke(viewModel);
        }

        public async UniTask CloseAllViewsExcept<T>(CancellationToken cancellationToken,
            bool tryUseFade = true) where T : IUIViewModel
        {
            var tasks = _views
                .Where(view => view.Key is not T)
                .Select(view => CloseViewAsync(view.Key, cancellationToken, tryUseFade)).ToList();

            await UniTask.WhenAll(tasks);
        }

        public event Action<IUIViewModel> OnBeforeOpenView;
        public event Action<IUIViewModel> OnAfterOpenView;
        public event Action<IUIViewModel> OnBeforeCloseView;
        public event Action<IUIViewModel> OnAfterCloseView;

        public void AddToContext(string name, RectTransform transform)
        {
            ShibariContext.Add(name, transform);
        }

        public void RemoveFromContext(string name)
        {
            ShibariContext.Remove(name);
        }

        public IUILocker UILocker => null;
    }
}