using System;
using System.Threading;
using R3;
using TapEmpire.Services;
using TapEmpire.UI;
using UnityEngine;

namespace TapEmpire.UI
{
    public class NoAdsPopupViewModel : IapPopupViewModel
    {
        public static readonly string IapKey = "no_ads_default";
        public string Placement { get; private set; } = String.Empty;

        public NoAdsPopupViewModel(string placement)
        {
            Placement = placement;
        }
    }
}