using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Notifications
{
    public interface ICustomNotificationRepository
    {
        Task<CustomNotification> CreateNotificationAsync(CustomNotification notification, CustomNotificationFilters filters, Guid userId);
        Task<CustomNotification> GetNotificationById(Guid notificationId);
        Task<List<CustomNotification>> GeNotifications();

        Task<List<CustomNotificationStatus>> GetDeliveryStatus(Guid notificationId);
    }
}
