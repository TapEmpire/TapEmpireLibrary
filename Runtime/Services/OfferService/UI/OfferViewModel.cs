
using System;

namespace TapEmpire.Services.Offer
{
    public class OfferViewModel : IapPopupViewModel
    {
        public string Placement { get; private set; } = String.Empty;
        public OfferRuntimeData OfferData { get; private set; } = null;

        public OfferViewModel(OfferRuntimeData data, string placement)
        {
            OfferData = data;
            Placement = placement;
        }

        public void SetOfferData(OfferRuntimeData data)
        {
            OfferData = data;
        }
    }
}