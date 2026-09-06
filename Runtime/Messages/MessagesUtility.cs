using System;
using System.Collections.Generic;
using TapEmpire.Utility;

namespace TapEmpire.Messages
{
    public static partial class MessagesUtility
    {
        private static readonly Dictionary<MessageType, List<Delegate>> LibraryCallbacks = new ();

        public static void Subscribe<T>(MessageType messageType, Action<T> callback) where T : IMessageData
            => Subscribe(messageType, callback, LibraryCallbacks);

        public static void Unsubscribe<T>(MessageType messageType, Action<T> callback) where T : IMessageData
            => Unsubscribe(messageType, callback, LibraryCallbacks);

        public static void Invoke<T>(MessageType messageType, T messageData) where T : IMessageData
            => Invoke(messageType, messageData, LibraryCallbacks);

        private static void Subscribe<TMessageType, T>(TMessageType messageType,
            Action<T> callback, Dictionary<TMessageType, List<Delegate>> callbackDictionary) where T : IMessageData
        {
            if (callbackDictionary.TryGetValue(messageType, out var callbacks))
            {
                callbacks.Add(callback);
            }
            else
            {
                callbackDictionary.Add(messageType, new List<Delegate>() { callback });
            }
        }

        private static void Unsubscribe<TMessageType, T>(TMessageType messageType, Action<T> callback,
            Dictionary<TMessageType, List<Delegate>> callbackDictionary) where T : IMessageData
        {
            if (callbackDictionary.TryGetValue(messageType, out var callbacks))
            {
                callbacks.Remove(callback);
            }
        }

        private static void Invoke<TMessageType, T>(TMessageType messageType, T messageData,
            Dictionary<TMessageType, List<Delegate>> callbackDictionary) where T : IMessageData
        {
            if (!callbackDictionary.TryGetValue(messageType, out var callbacks))
            {
                return;
            }
            using (ListScope<Delegate>.CreateFromEnumerable(callbacks, out var callbacksList))
            {
                foreach (var callback in callbacksList)
                {
                    (callback as Action<T>)?.Invoke(messageData);
                }
            }
        }
    }
}