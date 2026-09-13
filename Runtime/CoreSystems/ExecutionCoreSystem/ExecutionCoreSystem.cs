using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Services;
using Zenject;

namespace TapEmpire.CoreSystems
{
    public class ExecutionCoreSystem : Initializable, IExecutionCoreSystem, ITickable, IFixedTickable, ILateTickable
    {
        protected virtual IExecutionModule[] ExecutionModules { get; }

        public T GetModule<T>() where T : IExecutionModule
        {
            return ExecutionModules.OfType<T>().FirstOrDefault();
        }

        protected void InitializeModules(DiContainer diContainer)
        {
            foreach (var module in ExecutionModules)
            {
                diContainer.Inject(module);
                module.Initialize();
            }
        }

        public void Tick()
        {
            foreach (var module in ExecutionModules)
            {
                if (module is ITickable tickable)
                {
                    tickable.Tick();
                }
            }
        }

        public void FixedTick()
        {
            foreach (var module in ExecutionModules)
            {
                if (module is IFixedTickable tickable)
                {
                    tickable.FixedTick();
                }
            }
        }

        public void LateTick()
        {
            foreach (var module in ExecutionModules)
            {
                if (module is ILateTickable tickable)
                {
                    tickable.LateTick();
                }
            }
        }
    }
}
