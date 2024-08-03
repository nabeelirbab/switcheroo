using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.GraphQL.Notifications.Model
{
    public class CustomNotificationStatus
    {
        public CustomNotificationStatus(Guid? id, Guid notificationId, Guid userId, bool status)
        {
            Id = id;
            NotificationId = notificationId;
            UserId = userId;
            Status = status;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public Guid NotificationId { get; set; }
        [Required]
        public Guid UserId { get; set; }

        public bool Status { get; set; }

        public string? UserEmail { get; set; }
        public string? UserName { get; set; }

        public static CustomNotificationStatus FromDomain(Domain.Notifications.CustomNotificationStatus domainNotificationStatus)
        {
            if (!domainNotificationStatus.Id.HasValue) throw new ApiException("Mapping error. Id missing");

            return new CustomNotificationStatus(
                domainNotificationStatus.Id.Value,
                domainNotificationStatus.NotificationId,
                domainNotificationStatus.UserId,
                domainNotificationStatus.Status
                )
            {
                UserEmail = domainNotificationStatus.UserEmail,
                UserName = domainNotificationStatus.UserName
            };
        }
        public static List<CustomNotificationStatus> FromDomains(List<Domain.Notifications.CustomNotificationStatus> domainNotificationStatuses)
        {
            List<CustomNotificationStatus> statuses = new List<CustomNotificationStatus>();
            foreach (var domainStatus in domainNotificationStatuses)
            {
                var status = new CustomNotificationStatus(
                domainStatus.Id.Value,
                domainStatus.NotificationId,
                domainStatus.UserId,
                domainStatus.Status
                )
                {
                    UserEmail = domainStatus.UserEmail,
                    UserName = domainStatus.UserName
                };
                statuses.Add(status);
            }

            return statuses;
        }
    }
}
