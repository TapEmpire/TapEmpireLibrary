using System;

namespace TapEmpire.Services.Notifications
{
    public interface IPushNotificationsService : IService
    {
        public void SendNotification(string title, string body, DateTime deliveryTime, int? badgeNumber = null, bool reschedule = false);
        public GameNotification CreateNotification();
        public PendingNotification ScheduleNotification(GameNotification notification, DateTime deliveryTime);
        public void CancelNotification(int notificationId);
        public void CancelAllNotifications();
        public void DismissNotification(int notificationId);
        public void DismissAllNotifications();
        public GameNotification GetLastNotification();
    }
}