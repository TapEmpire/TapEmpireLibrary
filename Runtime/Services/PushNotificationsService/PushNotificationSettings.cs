using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapEmpire.Services.Notifications
{
    [CreateAssetMenu(menuName = "TapEmpire/Settings/PushNotificationSettings", fileName = "PushNotificationSettings")]
    public class PushNotificationSettings : ScriptableObject
    {
        [SerializeReference] public List<INotificationHandler> NotificationHandlers = new();
        
        public TimeSerializable DayStartTime;
        public TimeSerializable DayEndTime;
    }

    [Serializable]
    public class TimeSerializable
    {
        public int Hours;
        public int Minutes;
    }
}