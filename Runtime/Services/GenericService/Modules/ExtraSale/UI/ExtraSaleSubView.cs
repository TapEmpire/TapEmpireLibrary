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
        [SerializeField] private Transform _extraSaleParent;
        [SerializeField] private ScrollSnap _scrollSnap;
        [SerializeField] private TouchTracker _touchTracker;

        private IDisposable _scrollDisposable = null;
        private IDisposable _touchDisposable = null;

        public ExtraSaleSubView Initialize(ExtraSaleModule module)
        {
            module.CreateExtraSales().ForEach(extra =>
            {
                extra.SetParent(_extraSaleParent);
                extra.localScale = Vector3.one;
            });

            _scrollDisposable = UniTaskUtility.ExecuteNextFrame(() => SetAutoScroll(module));

            return this;
        }

        public void Dispose()
        {
            _scrollDisposable?.Dispose();
            _touchDisposable?.Dispose();
        }

        private void SetAutoScroll(ExtraSaleModule module)
        {
            _scrollSnap.enabled = true;
            var settings = module.Settings;

            _scrollDisposable = new CancellableTask(async token =>
            {
                await UniTask.Yield(cancellationToken: token);
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