using System;
using Cysharp.Threading.Tasks;
using TapEmpire.UI;
using TapEmpire.Utility;
using UnityEngine;
using UnityEngine.UI.Extensions;
using R3;

namespace TapEmpire.Modules
{
    public class ExtraSaleSubView : MonoBehaviour, IDisposable
    {
        private const float MaxWaitForPagesSeconds = 2.0f;
        
        [SerializeField] private Transform _extraSaleParent;
        [SerializeField] private ScrollSnap _scrollSnap;
        [SerializeField] private TouchTracker _touchTracker;

        private IDisposable _scrollDisposable = null;
        private IDisposable _touchDisposable = null;

        public ExtraSaleSubView Initialize(ExtraSaleModule module)
        {
            var extraSales = module.CreateExtraSales();
            extraSales.ForEach(extra =>
            {
                extra.SetParent(_extraSaleParent);
                extra.localScale = Vector3.one;
            });

            SetAutoScroll(module, extraSales.Count);

            return this;
        }

        public void Dispose()
        {
            _scrollDisposable?.Dispose();
            _touchDisposable?.Dispose();
        }

        private void SetAutoScroll(ExtraSaleModule module, int createdItemsCount)
        {
            var settings = module.Settings;

            _scrollDisposable = new CancellableTask(async token =>
            {
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: token);

                if (createdItemsCount <= 1)
                {
                    return;
                }

                float startTime = Time.unscaledTime;
                while (_scrollSnap.Pages <= 1 && Time.unscaledTime - startTime < MaxWaitForPagesSeconds)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: token);
                }

                if (_scrollSnap.Pages <= 1)
                {
                    return;
                }
                
                for (int i = 1; i < _scrollSnap.Pages; ++i)
                {
                    await UniTask.WaitForSeconds(settings.ScrollDelay, cancellationToken: token);
                    _scrollSnap.ChangePage(i);
                    await UniTask.WaitForSeconds(0.4f, cancellationToken: token);
                }
            });

            _touchDisposable = _touchTracker.OnDown.Subscribe(_ =>
            {
                _scrollDisposable.Dispose();
                _touchDisposable.Dispose();
            });
        }
    }
}
