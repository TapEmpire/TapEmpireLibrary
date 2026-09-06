using System;
using R3;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ResourceRuntimeData<T> : IDisposable
    {
        public ReactiveProperty<int> Amount { get; private set; } = new();
        public ReactiveProperty<DateTime> NextReplenishDate { get; private set; } = new();

        public float ReplenishTimeLeft => GetReplenishTimeLeft();
        public bool IsFull => _isRestorable && Amount.Value >= _settings.MaxAmount;

        private IProgressService _progressService;
        private ResourceSettings<T> _settings;
        private string _resourceName;
        private CancellableTask _replenishTimer = null;

        private bool _isRestorable => _settings.ReplenishTime > 0;

        public ResourceRuntimeData(ResourceSettings<T> settings, IProgressService progressService)
        {
            _progressService = progressService;
            _settings = settings;
            _resourceName = _settings.ResourceType.ToString();

            Amount.Value = _progressService.GetResourceCount(_resourceName, _settings.InitialAmount);

            if (_isRestorable)
            {
                CheckAbsentTime();
            }
        }

        public void Dispose()
        {
            _replenishTimer?.Dispose();
        }

        public bool HasAmount(int amount)
        {
            return amount <= Amount.Value;
        }

        public int Add(int amount)
        {
            Amount.Value += amount;
            _progressService.SetResourceCount(_resourceName, Amount.Value);
            CheckRefillCancel();
            return Amount.Value;
        }

        public int Subtract(int amount)
        {
            Amount.Value = Mathf.Max(0, Amount.Value - amount);
            _progressService.SetResourceCount(_resourceName, Amount.Value);
            CheckRefill();
            return Amount.Value;
        }

        public int Set(int amount)
        {
            Amount.Value = amount;
            _progressService.SetResourceCount(_resourceName, Amount.Value);
            CheckRefill();
            return Amount.Value;
        }

        public void RefreshFromProgress()
        {
            Amount.Value = _progressService.GetResourceCount(_resourceName, _settings.InitialAmount);
        }

        public void RecheckAbsentTime()
        {
            if (_isRestorable)
            {
                _replenishTimer?.Dispose();
                _replenishTimer = null;
                CheckAbsentTime();
            }
        }

        private void CheckAbsentTime()
        {
            if (Amount.Value >= _settings.MaxAmount) return;

            var timeStamp = _progressService.GetResourceTimeStamp(_resourceName);

            if (timeStamp > DateTime.UtcNow)
            {
                timeStamp = DateTime.UtcNow;
            }

            var elapsed = (DateTime.UtcNow - timeStamp).TotalSeconds;

            var amountRestored = (int)(elapsed / _settings.ReplenishTime);
            amountRestored = Mathf.Min(_settings.MaxAmount - Amount.Value, amountRestored);
            Amount.Value += amountRestored;

            _progressService.SetResourceCount(_resourceName, Amount.Value);

            if (Amount.Value < _settings.MaxAmount)
            {
                var lastTimeStamp = timeStamp + TimeSpan.FromSeconds(amountRestored * _settings.ReplenishTime);
                CheckRefill(lastTimeStamp);
            }
        }

        private void CheckRefill()
        {
            if (_isRestorable)
            {
                CheckRefill(DateTime.UtcNow);
            }
        }

        private void CheckRefill(DateTime dateTime)
        {
            if (Amount.Value < _settings.MaxAmount && _replenishTimer == null)
            {
                _progressService.SetResourceTimeStamp(_resourceName, dateTime);
                NextReplenishDate.Value = dateTime + TimeSpan.FromSeconds(_settings.ReplenishTime);

                var timeLeft = NextReplenishDate.Value - DateTime.UtcNow;
                _replenishTimer = UniTaskUtility.Delay(timeLeft.RoundedSeconds(), Replenish);
            }
        }

        private void CheckRefillCancel()
        {
            if (_isRestorable && Amount.Value >= _settings.MaxAmount)
            {
                _replenishTimer?.Dispose();
                _replenishTimer = null;
            }
        }

        private void Replenish()
        {
            Add(1);
            _replenishTimer = null;
            CheckRefill(DateTime.UtcNow);
        }

        private float GetReplenishTimeLeft()
        {
            return Mathf.Max((NextReplenishDate.Value - DateTime.UtcNow).Seconds, 0);
        }
    }
}