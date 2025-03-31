using System;
using UnityEngine.Events;

namespace TapEmpire.Utility
{
    public static class UnityEventExtensions
    {
        public static IDisposable Subscribe(this UnityEvent self, UnityAction action)
        {
            if (self == null || action == null)
            {
                throw new ArgumentNullException($"Invalid subscription {nameof(self)} {nameof(action)}");
            }

            return new EventSubscription(self, action);
        }

        private class EventSubscription : IDisposable
        {
            private UnityEvent _unityEvent;
            private UnityAction _action;
            private bool _isDisposed = false;

            public EventSubscription(UnityEvent unityEvent, UnityAction action)
            {
                _unityEvent = unityEvent;
                _action = action;

                _unityEvent.AddListener(_action);
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _unityEvent.RemoveListener(_action);
                    _isDisposed = true;
                }
            }
        }
    }
}