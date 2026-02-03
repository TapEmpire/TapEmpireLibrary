using Zenject;
using R3;
using TapEmpire.Services;
using UnityEngine;
using System.Linq;
using System;

namespace TapEmpire.Modules
{
    [Serializable]
    public class SegmentationModule : IGenericServiceModule
    {
        [field: SerializeField] public SegmentationSettings Settings { get; private set; }

        private IProgressService _progressService;
        private IAdsService _adsService;
        private CompositeDisposable _disposables = new();

        public void Initialize(DiContainer diContainer)
        {
            _progressService = diContainer.Resolve<IProgressService>();
            _adsService = diContainer.Resolve<IAdsService>();

            var analyticsService = diContainer.Resolve<IAnalyticsService>();
            analyticsService.CampaignName.Skip(1).Subscribe(OnCampaignName).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnCampaignName(string campaignName)
        {
            Settings.UpdateData();
            Debug.LogError("OnCampaignName");

            if (Settings.AdsSettings.EnableBanners == false)
            {
                _adsService.DisableBanners();
            }
        }
    }
}