using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class CustomNotificationStatus
    {
        public CustomNotificationStatus(Guid notificationId, Guid userId, bool status)
        {
            NotificationId = notificationId;
            UserId = userId;
            Status = status;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]

        public Guid NotificationId { get; set; }
        [Required]
        public Guid UserId { get; set; }

        public bool Status { get; set; }

        [NotMapped]
        public string? UserEmail { get; set; }
        [NotMapped]
        public string? UserName { get; set; }
        public void FromDomain(Domain.Notifications.CustomNotificationStatus domainNotificationStatus)
        {
            NotificationId = domainNotificationStatus.NotificationId;
            UserId = domainNotificationStatus.UserId;
            Status = domainNotificationStatus.Status;
        }

        public static List<Domain.Notifications.CustomNotificationStatus> ToDomains(List<CustomNotificationStatus> statuses)
        {
            List<Domain.Notifications.CustomNotificationStatus> domainStatuses = new List<Domain.Notifications.CustomNotificationStatus>();
            foreach (var status in statuses)
            {
                var domainStatusOjbect = new Domain.Notifications.CustomNotificationStatus(status.Id, status.NotificationId, status.UserId, status.Status)
                {
                    UserEmail = status.UserEmail,
                    UserName = status.UserName,
                };
                domainStatuses.Add(domainStatusOjbect);
            }
            return domainStatuses;
        }
        public static Expression<Func<CustomNotificationStatus, Domain.Notifications.CustomNotificationStatus>> ToDomain =>
            notification => new Domain.Notifications.CustomNotificationStatus(
                notification.Id,
                notification.NotificationId,
                notification.UserId,
                notification.Status
            );

    }
}
