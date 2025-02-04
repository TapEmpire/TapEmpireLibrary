using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services.Localization;
using TapEmpire.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class NoAdsPopupView : UIView<NoAdsPopupViewModel>, IInjectable
    {
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private string _buyLocalizationEntryName;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _closeButton.onClick.AddListener(CloseView);
            ConfigureIap();
            return base.OpenAsync(cancellationToken);
        }
        
        private void ConfigureIap()
        {
            var localizedPrice = DerivedModel.GetPrice();
            _priceText.text = string.IsNullOrEmpty(_buyLocalizationEntryName) ? 
                $"BUY {localizedPrice}" : 
                $"{LocalizationService.GetLocalizedString(LocalizationConstants.UITable, _buyLocalizationEntryName)} {localizedPrice}";
            
            _purchaseButton.onClick.AddListener(Purchase);
        }

        private void CloseView()
        {
            DerivedModel.Close();
        }

        private void Purchase()
        {
            _purchaseButton.onClick.RemoveAllListeners();
            DerivedModel.StartPurchase();
        }
    }
}