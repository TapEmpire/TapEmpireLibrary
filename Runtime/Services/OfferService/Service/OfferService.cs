using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services.Offer
{
    public class OfferService : Initializable, IOfferService
    {
        // [SerializeField] private ShopSettings _shopSettings;

        // public ShopSettings ShopSettings => _shopSettings;

        // public Observable<Unit> OnShopChanged => _onShopChanged;
        // public ReadOnlyReactiveProperty<bool> AreFreeItemsAvailable => _areFreeItemsAvailable;
        // public ReadOnlyReactiveProperty<(OfferData, DateTime)> ActiveOffer => _activeOffer;

        // private DiContainer _diContainer;
        // private IProgressService _progressService;
        // private IIapService _iapService;

        // private Subject<Unit> _onShopChanged = new();
        // private ReactiveProperty<(OfferData Data, DateTime TimeStamp)> _activeOffer = new();
        // private ReactiveProperty<bool> _areFreeItemsAvailable = new(false);
        // private CancellableTask _offerTimer = null;
        // private CancellableTask _midnightTimer = null;

        // private List<string> _freeItemsKeys = new();
        // private CompositeDisposable _disposables = new();

        // [Inject]
        // private void Construct(IProgressService progressService, IIapService iapService, DiContainer diContainer)
        // {
        //     _diContainer = diContainer;
        //     _progressService = progressService;
        //     _iapService = iapService;
        // }

        // protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        // {
        //     _freeItemsKeys = _shopSettings.Sections
        //         .OfType<CommonSectionData>()
        //         .SelectMany(section => section.Products)
        //         .Where(product => product.Type == ProductType.Free || product.Type == ProductType.Ads)
        //         .Select(product => product.Key)
        //         .ToList();

        //     UpdateCurrentOffer();
        //     SetMidnightTimer();

        //     _iapService.OnPurchaseSuccess.Subscribe(OnPurchaseSuccess).AddTo(_disposables);
        //     return base.OnInitializeAsync(cancellationToken);
        // }

        // protected override void OnRelease()
        // {
        //     _offerTimer?.Dispose();
        //     _midnightTimer?.Dispose();
        //     _disposables.Dispose();
        //     base.OnRelease();
        // }

        // public void SetTimeStamp(string key)
        // {
        //     _progressService.SetCurrentTimeStamp(key);
        //     UpdateFreeItems();
        // }

        // public (bool, TimeSpan) HasTimeStampToday(string key)
        // {
        //     var timestamp = _progressService.GetTimeStampDefault(key);
        //     var isToday = timestamp.IsTodayUTC();
        //     var elapsed = isToday ? MiscExtensions.GetTimeFromMidnight() : default;

        //     return (isToday, elapsed);
        // }

        // public void ResetTimers()
        // {
        //     _freeItemsKeys.ForEach(key => _progressService.CleanTimeStamp(key));
        //     UpdateFreeItems();
        // }

        // public void RewindTimers()
        // {
        //     var savedData = _progressService.GetOfferData();
        //     var timeStamp = DateTime.UtcNow - TimeSpan.FromDays(1) + TimeSpan.FromSeconds(20);
        //     _progressService.SetOfferData(savedData.Key, timeStamp);
        //     UpdateCurrentOffer();
        // }

        // private void UpdateFreeItems()
        // {
        //     _areFreeItemsAvailable.Value = _freeItemsKeys.Any(key => !HasTimeStampToday(key).Item1);
        // }

        // private void SetMidnightTimer()
        // {
        //     UpdateFreeItems();
        //     var timeLeft = MiscExtensions.GetTimeTillMidnight();
        //     _midnightTimer = UniTaskUtility.Delay(timeLeft.RoundedSeconds() + 1, SetMidnightTimer);
        // }

        // private void UpdateCurrentOffer()
        // {
        //     _activeOffer.Value = GetCurrentOffer();
        //     _onShopChanged.OnNext(Unit.Default);

        //     if (_activeOffer.Value.Data != null)
        //     {
        //         var timeLeft = _activeOffer.Value.TimeStamp + TimeSpan.FromDays(1) - DateTime.UtcNow;
        //         _offerTimer?.Dispose();
        //         _offerTimer = UniTaskUtility.Delay(timeLeft.RoundedSeconds() + 1, UpdateCurrentOffer);
        //     }
        // }

        // private (OfferData, DateTime) GetCurrentOffer()
        // {
        //     var offers = _shopSettings.Sections
        //         .OfType<OfferSectionData>()
        //         .SelectMany(section => section.OfferData)
        //         .Where(offer => offer.BundleType == BundleType.Swap).ToList();
        //     var savedData = _progressService.GetOfferData();

        //     if (offers.Count == 0)
        //     {
        //         return (null, default);
        //     }

        //     var currentIndex = offers.FindIndex(offerData => offerData.Name == savedData.Key);

        //     if (currentIndex == -1)
        //     {
        //         _progressService.SetOfferData(offers[0].Name, DateTime.UtcNow);
        //         return (offers[0], DateTime.UtcNow);
        //     }

        //     var elapsed = DateTime.UtcNow - savedData.TimeStamp;
        //     int daysPassed = (int)elapsed.TotalDays;
        //     int index = (currentIndex + daysPassed) % offers.Count;
        //     var timeStamp = savedData.TimeStamp;

        //     if (daysPassed > 0)
        //     {
        //         timeStamp = savedData.TimeStamp + TimeSpan.FromDays(daysPassed);
        //         _progressService.SetOfferData(offers[index].Name, timeStamp);
        //     }

        //     return (offers[index], timeStamp);
        // }

        // private void OnPurchaseSuccess(string productId)
        // {
        //     var hasKey = _activeOffer.Value.Data.Products.Any(product => product == productId);

        //     if (hasKey)
        //     {
        //         var offers = _shopSettings.Sections
        //             .OfType<OfferSectionData>()
        //             .SelectMany(section => section.OfferData)
        //             .Where(offer => offer.BundleType == BundleType.Swap).ToList();
        //         var currentIndex = offers.FindIndex(offerData => offerData.Name == _activeOffer.Value.Data.Name);
        //         currentIndex = MathUtility.LoopValue(currentIndex, offers.Count);
        //         _progressService.SetOfferData(offers[currentIndex].Name, DateTime.UtcNow);
        //         UpdateCurrentOffer();
        //     }
        // }
    }
}
