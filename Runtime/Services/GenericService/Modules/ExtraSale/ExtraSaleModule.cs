using Zenject;
using R3;
using TapEmpire.Services;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using TapEmpire.Services.Shop;
using WordGame.UI;
using WordGame.Services.Shop;
using TapEmpire.Services.Offer;

namespace TapEmpire.Modules
{
    [Serializable]
    public class ExtraSaleModule : IGenericServiceModule
    {
        [SerializeField] private ExtraSaleSettings _settings;

        private ShopSettings _shopSettings;
        private DiContainer _diContainer;
        private IProgressService _progressService;
        private int _coinsIndex = 0;
        private CompositeDisposable _disposables = new();

        public void Initialize(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _progressService = diContainer.Resolve<IProgressService>();
            var iapService = diContainer.Resolve<IIapService>();

            _shopSettings = diContainer.Resolve<IShopService>().ShopSettings;

            _coinsIndex = _progressService.GetTopCoinsPurchased();
            iapService.OnPurchaseSuccess.Subscribe(OnPurchase).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public List<Transform> CreateExtraSales()
        {
            return _settings.SaleList.Select(element => CreateExtraSale(element)).ToList();
        }

        private void OnPurchase(string purchaseId)
        {
            const string prefix = "coins";
            var coinsIndex = purchaseId.StartsWith(prefix) &&
                int.TryParse(purchaseId.Substring(prefix.Length), out var value) ? value : 0;

            if (coinsIndex > _coinsIndex)
            {
                _coinsIndex = coinsIndex;
                _progressService.SetTopCoinsPurchased(_coinsIndex);
            }
        }

        private Transform CreateExtraSale(string name)
        {
            switch (name)
            {
                case "packs": return CreatePacks(_settings.Packs);
                case "packs_flex": return CreatePacks(_settings.FlexPacks[_coinsIndex].Iaps);
                case "bundle_flex": return CreateBundle(_settings.Bundles[_progressService.GetRarity()]);
                case string bundle when bundle.StartsWith("bundle"): return CreateBundle(name);
                default: return null;
            }
        }

        private Transform CreatePacks(string[] packs)
        {
            var products = GetProducts(packs);
            var labels = GetLabels();

            var packsElement = GameObject.Instantiate(_settings.Visuals["packs"]);
            _diContainer.InjectGameObject(packsElement.gameObject);
            packsElement.GetComponent<PacksElement>().Initialize(products, labels);

            return packsElement;
        }

        private Transform CreateBundle(string bundleId)
        {
            var bundleData = GetBundleData(bundleId);
            var bundleElement = GameObject.Instantiate(_settings.Visuals[bundleId]);
            _diContainer.InjectGameObject(bundleElement.gameObject);
            bundleElement.GetComponent<BundleShopElement>().Initialize(bundleData);

            return bundleElement;
        }

        public List<ProductData> GetProducts(string[] keys)
        {
            var products = _shopSettings.Sections
                .OfType<CommonSectionData>()
                .SelectMany(section => section.Products);

            return keys
                .Select(key => products.FirstOrDefault(product => product.Key == key))
                .ToList();
        }

        public Services.Shop.OfferData GetBundleData(string bundleId)
        {
            return _shopSettings.Sections
                .OfType<OfferSectionData>()
                .SelectMany(section => section.OfferData)
                .First(offer => offer.Name == bundleId);
        }

        public List<Sprite> GetLabels()
        {
            var icons = _shopSettings.InfoIcons;
            return _settings.Labels.Select(label => icons[label]).ToList();
        }
    }
}