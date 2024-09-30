using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace TapEmpire.Utility
{
    public class TicksContainer : ITicksContainer, ITickable, IFixedTickable, ILateTickable
    {
        private DiContainer _diContainer;
        
        private readonly List<ITickable> _tickables = new();
        private readonly List<IFixedTickable> _fixedTickables = new();
        private readonly List<ILateTickable> _lateTickables = new();

        private TickableManager _tickableManager;

        private bool _initialized;

        public bool TryAddToTickableManager(TickableManager tickableManager)
        {
            if (_initialized || _tickableManager != null)
            {
                return false;
            }
            _tickableManager = tickableManager;
            _tickableManager.Add(this as ITickable);
            _tickableManager.AddFixed(this as IFixedTickable);
            _tickableManager.AddLate(this as ILateTickable);
            _initialized = true;
            return true;
        }

        public bool TryRemoveFromTickableManager()
        {
            if (_tickableManager == null || !_initialized)
            {
                return false;
            }
            _tickableManager.Remove(this as ITickable);
            _tickableManager.RemoveFixed(this as IFixedTickable);
            _tickableManager.RemoveLate(this as ILateTickable);
            _initialized = false;
            return true;
        }
        
        public void TryAddTicks<T>(T[] targets) where T : class
        {
            foreach (var target in targets)
            {
                TryAddTicks(target);
            }
        }

        public void TryAddTicks<T>(T target) where T : class
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
        
        public void TryRemoveTicks<T>(T[] targets) where T : class
        {
            foreach (var target in targets)
            {
                TryRemoveTicks(target);
            }
        }
        
        public void TryRemoveTicks<T>(T target) where T : class
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