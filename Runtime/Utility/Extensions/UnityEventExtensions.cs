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

            return new UnityEventSubscription(self, action);
        }

        public static IDisposable Subscribe<T>(this UnityEvent<T> self, UnityAction<T> action)
        {
            if (self == null || action == null)
            {
                throw new ArgumentNullException($"Invalid subscription {nameof(self)} {nameof(action)}");
            }

            return new UnityEventSubscription<T>(self, action);
        }

        public static IDisposable Subscribe<T>(this Action<T> self, Action<T> action)
        {
            if (self == null || action == null)
            {
                throw new ArgumentNullException($"Invalid subscription {nameof(self)} {nameof(action)}");
            }

            return new EventSubscription<T>(self, action);
        }

        private class UnityEventSubscription : IDisposable
        {
            private UnityEvent _unityEvent;
            private UnityAction _action;
            private bool _isDisposed = false;

            public UnityEventSubscription(UnityEvent unityEvent, UnityAction action)
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

        private class UnityEventSubscription<T> : IDisposable
        {
            private UnityEvent<T> _unityEvent;
            private UnityAction<T> _action;
            private bool _isDisposed = false;

            public UnityEventSubscription(UnityEvent<T> unityEvent, UnityAction<T> action)
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

        private class EventSubscription<T> : IDisposable
        {
            private event Action<T> _event;
            private Action<T> _action;
            private bool _isDisposed = false;

            public EventSubscription(Action<T> unityEvent, Action<T> action)
            {
                _event = unityEvent;
                _action = action;

                _event += _action;
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _event -= _action;
                    _isDisposed = true;
                }
            }
        }
    }
}