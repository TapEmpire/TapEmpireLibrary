using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class GridShopSection : ShopSection
    {
        public override void Initialize(DiContainer diContainer, SectionData sectionData)
        {
            var data = sectionData as CommonSectionData;
            foreach (var product in data.Products)
            {
                var element = GameObject.Instantiate(data.ShopElement, transform);
                if (diContainer != null)
                {
                    _elements.Add(element);
                    diContainer.InjectGameObject(element.gameObject);
                    element.Initialize(product);
                }
            }

            base.Initialize(diContainer, sectionData);
        }

        protected override void CalculateHeight()
        {
            GetComponent<RectTransform>().SetHeight(_layoutGroup.CalculateHeightAsGrid(_elements.Count));
        }
    }
}
