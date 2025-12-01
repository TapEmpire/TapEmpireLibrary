using System.Linq;
using R3;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class OfferShopSection : ShopSection
    {
        private CompositeDisposable _disposables = new();

        public override void Initialize(DiContainer diContainer, SectionData sectionData)
        {
            var shopService = diContainer.Resolve<IShopService>();
            var adsService = diContainer.Resolve<IAdsService>();
            var activeOffer = shopService.ActiveOffer.CurrentValue;

            var data = sectionData as OfferSectionData;

            var offers = data.OfferData.Where(offer => offer.BundleType == BundleType.Always ||
                (offer.BundleType == BundleType.NoAds && !adsService.AdsDisabled));
            offers = offers.Prepend(activeOffer.Data).ToList();

            foreach (var offer in offers)
            {
                var element = GameObject.Instantiate(offer.ShopElement, transform);
                if (diContainer != null)
                {
                    _elements.Add(element);
                    diContainer.InjectGameObject(element.gameObject);
                    element.Initialize(offer);

                    element.OnShouldDestroy.Subscribe(OnShouldDestroy).AddTo(_disposables);
                }
            }

            this.gameObject.SetActive(_elements.Count > 0);

            base.Initialize(diContainer, sectionData);
        }

        protected override void CalculateHeight()
        {
            GetComponent<RectTransform>().SetHeight(
                    _layoutGroup.CalculateHeightAsVertical(_elements.Select(element => element.GetComponent<RectTransform>())));
        }

        private void OnShouldDestroy(BaseShopElement shopElement)
        {
            _elements.Remove(shopElement);
            Destroy(shopElement.gameObject);

            this.gameObject.SetActive(_elements.Count > 0);

            if (_elements.Count > 0)
            {
                CalculateHeight();
            }
        }
    }
}
