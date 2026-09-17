using System.Collections.Generic;
using Zenject;

namespace TapEmpire.Utility
{
    public class TicksContainer : ITicksContainer, ITickable, IFixedTickable, ILateTickable
    {
        private readonly List<ITickable> _tickables = new();
        private readonly List<IFixedTickable> _fixedTickables = new();
        private readonly List<ILateTickable> _lateTickables = new();
        private readonly List<IAlwaysTickable> _alwaysTickables = new();

        private TickableManager _tickableManager;

        private bool _initialized;

        public bool Initialized => _initialized;

        public bool IsPaused { get; set; }

        public void TryInitialize(TickableManager tickableManager)
        {
            if (_initialized || _tickableManager != null)
            {
                return;
            }
            _tickableManager = tickableManager;
            _tickableManager.Add(this as ITickable);
            _tickableManager.AddFixed(this as IFixedTickable);
            _tickableManager.AddLate(this as ILateTickable);
            _initialized = true;
        }

        public void TryRelease()
        {
            if (!_initialized || _tickableManager == null)
            {
                return;
            }

            //todo: this is hotfix, refactor in future. release ticks_container at app exit
            if (_tickables.Count > 0 || _fixedTickables.Count > 0 || _lateTickables.Count > 0)
                return;
            
            _tickableManager.Remove(this as ITickable);
            _tickableManager.RemoveFixed(this as IFixedTickable);
            _tickableManager.RemoveLate(this as ILateTickable);
            _tickableManager = null;
            _initialized = false;
        }

        void ITicksContainer.TryAddTicks<T>(T target)
        {
            if (target is ITickable tickable && !_tickables.Contains(tickable))
            {
                _tickables.Add(tickable);
            }
            if (target is IFixedTickable fixedTickable && !_fixedTickables.Contains(fixedTickable))
            {
                _fixedTickables.Add(fixedTickable);
            }
            if (target is ILateTickable lateTickable && !_lateTickables.Contains(lateTickable))
            {
                _lateTickables.Add(lateTickable);
            }
            if (target is IAlwaysTickable alwaysTickable && !_alwaysTickables.Contains(alwaysTickable))
            {
                _alwaysTickables.Add(alwaysTickable);
            }
        }

        void ITicksContainer.TryRemoveTicks<T>(T target)
        {
            if (target is ITickable tickable && _tickables.Contains(tickable))
            {
                _tickables.Remove(tickable);
            }
            if (target is IFixedTickable fixedTickable && _fixedTickables.Contains(fixedTickable))
            {
                _fixedTickables.Remove(fixedTickable);
            }
            if (target is ILateTickable lateTickable && _lateTickables.Contains(lateTickable))
            {
                _lateTickables.Remove(lateTickable);
            }
            if (target is IAlwaysTickable alwaysTickable && _alwaysTickables.Contains(alwaysTickable))
            {
                _alwaysTickables.Remove(alwaysTickable);
            }
        }

        public void Tick()
        {
            foreach (var alwaysTickable in _alwaysTickables)
            {
                alwaysTickable.AlwaysTick();
            }

            if (IsPaused) return;

            foreach (var tickable in _tickables)
            {
                tickable.Tick();
            }
        }

        public void FixedTick()
        {
            if (IsPaused) return;

            foreach (var fixedTickable in _fixedTickables)
            {
                fixedTickable.FixedTick();
            }
        }

        public void LateTick()
        {
            if (IsPaused) return;

            foreach (var lateTickable in _lateTickables)
            {
                lateTickable.LateTick();
            };
        }
    }
}