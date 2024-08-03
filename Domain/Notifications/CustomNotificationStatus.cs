using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Notifications
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

        [NotMapped]
        public string? UserEmail { get; set; }
        [NotMapped]
        public string? UserName { get; set; }
        public static CustomNotificationStatus CreateNewNotification(
            Guid notificationId,
            Guid userId,
            bool status
        )
        {
            return new CustomNotificationStatus(
                null,
                notificationId,
                userId,
                status
            );
        }
    }
}
