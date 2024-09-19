using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Notifications
{
    public interface ISystemNotificationRepository
    {
        Task<SystemNotification> CreateAsync(SystemNotification notification, bool sendNotification = true, Dictionary<string, string> notificationData = null);
        Task<bool> MarkAsRead(Guid notiicationId);
        Task<SystemNotification> GetById(Guid id);
        Task<List<SystemNotification>> GetAll();
        Task<List<SystemNotification>> GetUnread();
        Task<List<SystemNotification>> GetByUserId(Guid userId);
        Task<List<SystemNotification>> GetUnreadByUserId(Guid userId);
    }
}
