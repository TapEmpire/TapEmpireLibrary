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

namespace TapEmpire.UI
{
    [Serializable]
    public class UIService : Initializable, IUIService
    {
        [SerializeField]
        private GameObject _canvasPrefab;

        [NonSerialized]
        private RectTransform _canvasRectTransform;

        [Inject]
        private DiContainer _servicesContainer;

        [Inject]
        private ISceneContextsService _sceneContextsService;

        [NonSerialized]
        private Dictionary<IUIViewModel, UIView> _views = new();

        private IUIViewModel _currentPopupModel;
        private DiContainer _coreDiContainer;

        public Dictionary<string, RectTransform> ShibariContext { get; private set; } = new();

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

        public async UniTask OpenViewAsync(UIView viewPrefab, IUIViewModel viewModel, CancellationToken cancellationToken, bool tryUseDefaultFadeIn = true,
            bool openAsPopup = false)
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
            OnBeforeOpenView?.Invoke(viewModel);
            if (openAsPopup && _currentPopupModel != null)
            {
                CloseViewAsync(_currentPopupModel, cancellationToken, false).Forget();
                _currentPopupModel = null;
            }
            var view = Object.Instantiate(viewPrefab, _canvasRectTransform);
            if (view is IInjectableView && _coreDiContainer != null)
            {
                _coreDiContainer.Inject(view);
            }
            view.Model = viewModel;
            _views.Add(viewModel, view);
            if (tryUseDefaultFadeIn && view is IFadeAbleView fadeAbleView)
            {
                fadeAbleView.CanvasGroup.alpha = 0;
                //DoTweenUtility.Fade(fadeAbleView.CanvasGroup, 1, _defaultFadeInData);
            }
            await view.OpenAsync(cancellationToken);
            if (openAsPopup)
            {
                _currentPopupModel = viewModel;
            }
            OnAfterOpenView?.Invoke(viewModel);
        }

        public async UniTask CloseViewAsync(IUIViewModel viewModel, CancellationToken cancellationToken, bool tryUseDefaultFadeOut = true)
        {
            if (!_views.TryGetValue(viewModel, out var view))
            {
                return;
            }
            OnBeforeCloseView?.Invoke(viewModel);
            if (tryUseDefaultFadeOut && view is IFadeAbleView fadeAbleView)
            {
                //DoTweenUtility.Fade(fadeAbleView.CanvasGroup, 0, _defaultFadeOutData);

                using (ListScope<UniTask>.Create(out var tasks))
                {
                    //tasks.Add(UniTask.WaitForSeconds(_defaultFadeOutData.Duration, cancellationToken:cancellationToken));

                    tasks.Add(view.CloseAsync(cancellationToken));

                    await UniTask.WhenAll(tasks);
                }
            }
            else
            {
                await view.CloseAsync(cancellationToken);
            }
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
            bool tryUseDefaultFadeOut = true) where T : IUIViewModel
        {
            var tasks = _views
                .Where(view => view.Key is not T)
                .Select(view => CloseViewAsync(view.Key, cancellationToken, tryUseDefaultFadeOut)).ToList();

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