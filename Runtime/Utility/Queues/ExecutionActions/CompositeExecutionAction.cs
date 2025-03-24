using System;
using R3;

namespace TapEmpire.Utility
{
    public class CompositeExecutionAction : ExecutionAction, IDisposable
    {
        private IExecutionAction[] _actions = Array.Empty<IExecutionAction>();

        private int _index = -1;

        public int Count => _actions.Length;

        public CompositeDisposable _disposables = new();

        public CompositeExecutionAction(IExecutionAction[] actions)
        {
            _actions = actions;
            _actions.ForEach(action => action.OnDone.Subscribe(OnDoneCallback).AddTo(_disposables));
        }

        public override void Execute()
        {
            OnDoneCallback(true);
        }

        public override void Reinitialize()
        {
            _actions.ForEach(action => action.Reinitialize());
            _index = -1;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnDoneCallback(bool isDone)
        {
            if (++_index >= _actions.Length)
            {
                MarkComplete();
                return;
            }

            _actions[_index].Execute();
        }
    }
}