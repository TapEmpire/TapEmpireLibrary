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
        
        private TickableManager Manager
        {
            get
            {
                if (_tickableManager == null)
                {
                    _tickableManager = _diContainer.Resolve<TickableManager>();
                }

                return _tickableManager;
            }
        }
        
        public TicksContainer(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }
        
        public void InitializeTicks<T>(T[] targets) where T : class
        {
            foreach (var target in targets)
            {
                InitializeTicks(target);
            }
        }

        public void InitializeTicks<T>(T target) where T : class
        {
            if (target is ITickable tickable && !_tickables.Contains(tickable))
            {
                if (_tickables.Count == 0)
                {
                    Manager.Add(this);
                }
                _tickables.Add(tickable);
            }
            if (target is IFixedTickable fixedTickable && !_fixedTickables.Contains(fixedTickable))
            {
                if (_fixedTickables.Count == 0)
                {
                    Manager.AddFixed(this);
                }
                _fixedTickables.Add(fixedTickable);
            }
            if (target is ILateTickable lateTickable && !_lateTickables.Contains(lateTickable))
            {
                if (_lateTickables.Count == 0)
                {
                    Manager.AddLate(this);
                }
                _lateTickables.Add(lateTickable);
            }
        }
        
        public void ReleaseTicks<T>(T[] targets) where T : class
        {
            foreach (var target in targets)
            {
                ReleaseTicks(target);
            }
        }
        
        public void ReleaseTicks<T>(T target) where T : class
        {
            if (target is ITickable tickable && _tickables.Contains(tickable))
            {
                _tickables.Remove(tickable);
                if (_tickables.Count == 0)
                {
                    Manager.Remove(this);
                }
            }
            if (target is IFixedTickable fixedTickable && _fixedTickables.Contains(fixedTickable))
            {
                _fixedTickables.Remove(fixedTickable);
                if (_fixedTickables.Count == 0)
                {
                    Manager.RemoveFixed(this);
                }
            }
            if (target is ILateTickable lateTickable && _lateTickables.Contains(lateTickable))
            {
                _lateTickables.Remove(lateTickable);
                if (_lateTickables.Count == 0)
                {
                    Manager.RemoveLate(this);
                }
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