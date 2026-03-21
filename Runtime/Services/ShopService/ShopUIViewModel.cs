using System;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class ShopUIViewModel : IUIViewModel, IInjectable, IDisposable
    {
        public ShopSettings ShopSettings { get; private set; }
        public DiContainer DiContainer { get; private set; }
        public IIapService IapService;
        public IShopService ShopService;
        public IAdsService AdsService;

        public bool HasCloseButton { get; }
        public Action OnSettingsPressed { get; }

        private IUIService _uiService;

        private ResourcesBar _resourcesBar;
        private CompositeDisposable _disposables = new();

        public event Action OnClose;

        public ShopUIViewModel(bool hasCloseButton = true, Action onSettingsPressed = null)
        {
            HasCloseButton = hasCloseButton;
            OnSettingsPressed = onSettingsPressed;
        }
        
        [Inject]
        private void Construct(DiContainer diContainer, IUIService uiService, IIapService iapService,
            IShopService shopService, IAdsService adsService)
        {
            DiContainer = diContainer;
            IapService = iapService;
            ShopService = shopService;
            AdsService = adsService;
            _uiService = uiService;

            _resourcesBar = _uiService.ShibariContext.TryGetValue("Resources").GetComponent<ResourcesBar>();
            _resourcesBar.MoveFront();

            ShopSettings = shopService.ShopSettings;
        }

        public void Dispose()
        {
            _resourcesBar.MoveBack();
            _disposables.Dispose();
        }

        public void OnClosePressed()
        {
            OnClose?.Invoke();
            OnClose = null;
            _uiService.CloseViewAsync(this, default).Forget();
        }
    }
}