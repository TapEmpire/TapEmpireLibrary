using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services.Localization;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class ShopSection : MonoBehaviour, IDisposable
    {
        [SerializeField] protected LayoutGroup _layoutGroup;
        [SerializeField] protected TMP_Text _title;

        protected bool IsGridLayout => _layoutGroup is GridLayoutGroup;
        protected List<BaseShopElement> _elements = new();

        public virtual void Initialize(DiContainer diContainer, SectionData sectionData)
        {
            _title.text = new LocalizedString(LocalizationConstants.UITable, sectionData.Name.ToLower()).GetLocalizedString();
            CalculateHeight();
        }

        public void Dispose()
        {
            _elements.ForEach(element => Destroy(element.gameObject));
            _elements.Clear();

            GetComponent<RectTransform>().SetHeight(GetHeight());
        }

        public float GetHeight()
        {
            return _layoutGroup.padding.top + _layoutGroup.padding.bottom + 0.0f;
        }

        protected virtual void CalculateHeight()
        {
            GetComponent<RectTransform>().SetHeight(_elements.Sum(element => element.Height));
        }
    }
}
