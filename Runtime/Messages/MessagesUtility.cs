using System;
using System.Collections.Generic;
using TapEmpire.Utility;

namespace TapEmpire.Messages
{
    public static partial class MessagesUtility
    {
        private static readonly Dictionary<TelMessageType, List<Delegate>> LibraryCallbacks = new ();

        public static void Subscribe<T>(TelMessageType messageType, Action<T> callback) where T : IMessageData
            => Subscribe(messageType, callback, LibraryCallbacks);

        public static void Unsubscribe<T>(TelMessageType messageType, Action<T> callback) where T : IMessageData
            => Unsubscribe(messageType, callback, LibraryCallbacks);

        public static void Invoke<T>(TelMessageType messageType, T messageData) where T : IMessageData
            => Invoke(messageType, messageData, LibraryCallbacks);

        private static void Subscribe<MessageType, T>(MessageType messageType,
            Action<T> callback, Dictionary<MessageType, List<Delegate>> callbackDictionary) where T : IMessageData
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

        private static void Unsubscribe<MessageType, T>(MessageType messageType, Action<T> callback,
            Dictionary<MessageType, List<Delegate>> callbackDictionary) where T : IMessageData
        {
            if (callbackDictionary.TryGetValue(messageType, out var callbacks))
            {
                callbacks.Remove(callback);
            }
        }

        private static void Invoke<MessageType, T>(MessageType messageType, T messageData,
            Dictionary<MessageType, List<Delegate>> callbackDictionary) where T : IMessageData
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