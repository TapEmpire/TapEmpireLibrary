using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using R3;
using TapEmpire.Services.Analytics;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Offer
{
    public class OfferAnalyticsModule : IDisposable
    {
        private readonly DiContainer _diContainer;
        private readonly IAnalyticsService _analyticsService;
        private readonly IOfferService _offerService;
        private readonly IProgressService _progressService;

        private CompositeDisposable _disposables = new();

        public OfferAnalyticsModule(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _analyticsService = _diContainer.Resolve<IAnalyticsService>();
            _offerService = _diContainer.Resolve<IOfferService>();
            _progressService = _diContainer.Resolve<IProgressService>();

            _offerService.OnOfferShown.Subscribe(OnOfferShown).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnOfferShown((OfferType OfferType, bool Autoshown, string Placement) data)
        {
            var level = _progressService.GetLevelProgress() + 1;

            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>()
            {
                { "Offers", new JObject(new JProperty(data.OfferType.ToString(),
                    new JObject(new JProperty(data.Placement, $"Level_{level}")))) }
            });
        }
    }
}