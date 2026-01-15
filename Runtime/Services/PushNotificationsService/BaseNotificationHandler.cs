#if TEL_NOTIFICATIONS

using System;
using TapEmpire.Services.Localization;
using TapEmpire.Services.Notifications;
using Zenject;

namespace TapEmpire.Services.Notifications
{
    [Serializable]
    public class BaseNotificationHandler : INotificationHandler
    {
        public string Title;
        public string Body;
        
        protected IPushNotificationsService _notificationsService;
        
        public virtual void Initialize(DiContainer diContainer)
        {
            _notificationsService = diContainer.Resolve<IPushNotificationsService>();
        }

        public virtual void Dispose()
        {
            
        }

        protected string GetLocalizedTitle()
        {
            return LocalizationService.GetLocalizedString(LocalizationConstants.PushNotificationsTable, Title);
        }
        
        protected string GetLocalizedBody()
        {
            return LocalizationService.GetLocalizedString(LocalizationConstants.PushNotificationsTable, Body);
        }
    }
}

#endif