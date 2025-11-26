
using System;

namespace TapEmpire.Services.Offer
{
    public class OfferViewModel : IapPopupViewModel
    {
        public string Placement { get; private set; } = String.Empty;
        public OfferRuntimeData OfferData { get; private set; } = null;
        public bool IsDebug { get; private set; } = false;

        public OfferViewModel(OfferRuntimeData data, string placement, bool isDebug)
        {
            OfferData = data;
            Placement = placement;
            IsDebug = isDebug;
        }

        public void SetOfferData(OfferRuntimeData data)
        {
            OfferData = data;
        }
    }
}