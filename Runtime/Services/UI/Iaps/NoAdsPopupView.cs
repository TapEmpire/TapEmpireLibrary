using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Services.Localization;
using TapEmpire.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace TapEmpire.UI
{
    public class NoAdsPopupView : UIView<NoAdsPopupViewModel>, IInjectable
    {
        [SerializeField] private string _iapKey = "no_ads_default";
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private LocalizedString _buyLocalization;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _closeButton.onClick.AddListener(CloseView);
            _purchaseButton.onClick.AddListener(Purchase);

            ConfigureIap();
            return base.OpenAsync(cancellationToken);
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _purchaseButton.onClick.RemoveAllListeners();

            return base.CloseAsync(cancellationToken);
        }

        private void ConfigureIap()
        {
            var localizedPrice = DerivedModel.GetPrice(_iapKey);
            var buyText = string.IsNullOrEmpty(_buyLocalization.TableEntryReference) ? "BUY" : _buyLocalization.GetLocalizedString();
            _priceText.text = $"{buyText} {localizedPrice}";
        }

        private void CloseView()
        {
            DerivedModel.Close();
        }

        private void Purchase()
        {
            DerivedModel.StartPurchase(_iapKey);
        }
    }
}