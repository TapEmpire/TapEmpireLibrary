using System;
using R3;
using TapEmpire.Utility;
using UnityEngine;

namespace TapEmpire.Services
{
    public class ResourceRuntimeData<T> : IDisposable
    {
        private readonly Subject<Unit> _onRanOut = new();
        private readonly Subject<(int AmountUsed, string Reason)> _onUseEnded = new();

        public ReactiveProperty<int> Amount { get; private set; } = new();
        public ReactiveProperty<DateTime> NextReplenishDate { get; private set; } = new();
        public ReactiveProperty<bool> IsTicking { get; private set; } = new();
        public Observable<Unit> OnRanOut => _onRanOut;
        public Observable<(int AmountUsed, string Reason)> OnUseEnded => _onUseEnded;

        public float ReplenishTimeLeft => GetReplenishTimeLeft();
        public bool IsFull => _isRestorable && Amount.Value >= _settings.MaxAmount;
        public int UsedAmount => IsTicking.Value ? _usedAmount : 0;

        private IProgressService _progressService;
        private ResourceSettings<T> _settings;
        private string _resourceName;
        private CancellableTask _replenishTimer = null;
        private CancellableTask _tickTimer = null;
        private readonly HoldCounter _tickingPauses = new();
        private int _usedAmount;

        private bool _isRestorable => _settings.ReplenishTime > 0;
        private bool _isMaxStrict => _settings.IsMaxStrict && _settings.MaxAmount > 0;

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
            _tickTimer?.Dispose();
        }

        public bool HasAmount(int amount)
        {
            return amount <= Amount.Value;
        }

        public bool IsFree(int value)
        {
            return value < _settings.FreeUntil;
        }

        public int Add(int amount)
        {
            Amount.Value = _isMaxStrict ? Mathf.Min(Amount.Value + amount, _settings.MaxAmount) : Amount.Value + amount;
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

        public void StartTicking()
        {
            if (_settings.Kind != ResourceKind.Tickable)
            {
                Debug.LogError($"{_resourceName} is not a tickable resource");
                return;
            }

            if (IsTicking.Value || Amount.Value == 0) return;

            _usedAmount = 0;
            IsTicking.Value = true;
            ApplyTickTimer();
        }

        public void StopTicking(string reason = "")
        {
            if (!IsTicking.Value) return;

            IsTicking.Value = false;
            ApplyTickTimer();
            _onUseEnded.OnNext((_usedAmount, reason));
        }

        public IDisposable PauseTicking()
        {
            var pause = _tickingPauses.Hold();
            ApplyTickTimer();

            return Disposable.Create(() =>
            {
                pause.Dispose();
                ApplyTickTimer();
            });
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

        private void Tick()
        {
            Subtract(1);
            ++_usedAmount;

            if (Amount.Value == 0)
            {
                StopTicking(ResourceUsageType.Auto);
                _onRanOut.OnNext(Unit.Default);
            }
        }

        private void ApplyTickTimer()
        {
            var shouldTick = IsTicking.Value && !_tickingPauses.IsHeld.CurrentValue;

            if (shouldTick && _tickTimer == null)
            {
                _tickTimer = UniTaskUtility.LoopedTask(1, Tick);
            }
            else if (!shouldTick && _tickTimer != null)
            {
                _tickTimer.Dispose();
                _tickTimer = null;
            }
        }

        private float GetReplenishTimeLeft()
        {
            return Mathf.Max((float)(NextReplenishDate.Value - DateTime.UtcNow).TotalSeconds, 0);
        }
    }
}