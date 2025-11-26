using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace TapEmpire.Services.Shop
{
    [System.Serializable]
    public struct ShopChoiceData
    {
        public Button Button;
        public TMP_Text Price;
        public List<VisualRewardData> Resources;
    }

    [System.Serializable]
    public struct VisualRewardData
    {
        public Image Icon;
        public TMP_Text Amount;
    }
}