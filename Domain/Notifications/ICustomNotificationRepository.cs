using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Notifications
{
    public interface ICustomNotificationRepository
    {
        Task<CustomNotification> CreateNotificationAsync(CustomNotification notification);
        Task<CustomNotification> GetNotificationById(Guid notificationId);
        Task<List<CustomNotification>> GeNotifications();
    }
}
