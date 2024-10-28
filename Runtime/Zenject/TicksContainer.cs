using System.Collections.Generic;
using Zenject;

namespace TapEmpire.Utility
{
    public class TicksContainer : ITicksContainer, ITickable, IFixedTickable, ILateTickable
    {
        private readonly List<ITickable> _tickables = new();
        private readonly List<IFixedTickable> _fixedTickables = new();
        private readonly List<ILateTickable> _lateTickables = new();

        private TickableManager _tickableManager;

        private bool _initialized;

        public bool Initialized => _initialized;

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
            _tickableManager.Remove(this as ITickable);
            _tickableManager.RemoveFixed(this as IFixedTickable);
            _tickableManager.RemoveLate(this as ILateTickable);
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
        }

        public void Tick()
        {
            foreach (var tickable in _tickables)
            {
                tickable.Tick();
            }
        }

        public void FixedTick()
        {
            foreach (var fixedTickable in _fixedTickables)
            {
                fixedTickable.FixedTick();
            }
        }

        public void LateTick()
        {
            foreach (var lateTickable in _lateTickables)
            {
                lateTickable.LateTick();
            };
        }
    }
}