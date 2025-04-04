using System;
using R3;

namespace TapEmpire.Utility
{
    public class CompositeExecutionAction<T> : ExecutionAction<T> where T : Enum
    {
        private IExecutionAction<T>[] _actions = Array.Empty<IExecutionAction<T>>();

        private int _index = -1;
        private T _flow;

        public int Count => _actions.Length;

        public CompositeDisposable _disposables = new();

        public CompositeExecutionAction(params IExecutionAction<T>[] actions)
        {
            _actions = actions;
            _actions.ForEach(action => action.OnDone.Subscribe(OnDoneCallback).AddTo(_disposables));
        }

        public override void Execute(T flow = default)
        {
            _flow = flow;
            OnDoneCallback(true);
        }

        public override void Reinitialize()
        {
            _actions.ForEach(action => action.Reinitialize());
            _index = -1;
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            base.Dispose();
        }

        public void Add(IExecutionAction<T> action)
        {
            // 
        }

        private void OnDoneCallback(bool isDone)
        {
            if (!isDone) return;

            if (++_index >= _actions.Length)
            {
                MarkComplete();
                return;
            }

            _actions[_index].Execute(_flow);
        }
    }
}