using System.Collections;
using System.Collections.Generic;
using R3;
using WordGame.Services;
using TapEmpire.Services;
using TapEmpire.UI;
using TapEmpire.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TapEmpire.Services.Shop
{
    public class ChoiceShopElement : SoftShopElement
    {
        public ReactiveCommand<ResourceType> OnResourceAdded { get; } = new();

        [SerializeField] private Button _adsButton;

        private IAdsService _adsService;

        [Inject]
        private void Construct2(IAdsService adsService)
        {
            _adsService = adsService;
        }

        public override void Initialize(ProductData data)
        {
            base.Initialize(data);
            _adsButton.onClick.Subscribe(OnClick).AddTo(_disposables);
        }

        private void OnClick()
        {
            _adsService.ShowRewarded($"{AdType.Resources_}{_data.Reward.Resource}", OnClickResult);
        }

        private void OnClickResult()
        {
            var from = _icon.transform.position;
            AcquireResources(_data.Reward.Resource, _data.Reward.Amount, ResourceUsage.PopupAds, from, true);
        }

        protected override void AcquireResources(ResourceType resourceType, int amount, ResourceUsage usage,
            Vector3 startPosition, bool shouldAddResource)
        {
            _resourcesService.Add(resourceType, amount, usage.ToString());
            OnResourceAdded.Execute(resourceType);
        }
    }
}
