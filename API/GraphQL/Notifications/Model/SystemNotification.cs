using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Notifications;

namespace API.GraphQL.Notifications.Model
{
    public class SystemNotification
    {
        public SystemNotification(Guid? id, string title, string message, NotificationType type, Guid userId, string? data, bool isRead, string navigateTo)
        {
            Id = id;
            Title = title;
            Message = message;
            Type = type;
            UserId = userId;
            Data = data;
            IsRead = isRead;
            NavigateTo = navigateTo;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public Guid UserId { get; set; }
        public string? Data { get; set; }
        public bool IsRead { get; set; }
        public string NavigateTo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }


        public static SystemNotification FromDomain(Domain.Notifications.SystemNotification domainNotification)
        {
            if (!domainNotification.Id.HasValue) throw new ApiException("Mapping error. Id missing");

            return new SystemNotification(
                domainNotification.Id.Value,
                domainNotification.Title,
                domainNotification.Message,
                domainNotification.Type,
                domainNotification.UserId,
                domainNotification.Data,
                domainNotification.IsRead,
                domainNotification.NavigateTo
                )
            {
                CreatedAt = domainNotification.CreatedAt,
                ReadAt = domainNotification.ReadAt
            };
        }
        public static List<SystemNotification> FromDomains(List<Domain.Notifications.SystemNotification> domainNotifications)
        {
            if (domainNotifications == null || domainNotifications.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<SystemNotification>();
            }
            return domainNotifications.Select(newNotification => new SystemNotification(
                newNotification.Id,
                newNotification.Title,
                newNotification.Message,
                newNotification.Type,
                newNotification.UserId,
                newNotification.Data,
                newNotification.IsRead, newNotification.NavigateTo)
            {
                CreatedAt = newNotification.CreatedAt,
                ReadAt = newNotification.ReadAt
            })
                .ToList();
        }


    }
}
