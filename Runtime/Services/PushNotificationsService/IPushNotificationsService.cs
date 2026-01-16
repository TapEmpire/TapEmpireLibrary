#if TEL_NOTIFICATIONS

using System;
using R3;

namespace TapEmpire.Services.Notifications
{
    public interface IPushNotificationsService : IService
    {
        ReadOnlyReactiveProperty<bool> OnFocusChanged { get; }
        PushNotificationSettings NotificationSettings { get; }

        public PendingNotification SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null, bool reschedule = false);
        public GameNotification CreateNotification();
        public PendingNotification ScheduleNotification(GameNotification notification, DateTime deliveryTime);
        public void CancelNotification(int notificationId);
        public void CancelAllNotifications();
        public void DismissNotification(int notificationId);
        public void DismissAllNotifications();
        public GameNotification GetLastNotification();
    }
}

#endif