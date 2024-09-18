using Domain.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class SystemNotification
    {
        public SystemNotification(string title, string message, NotificationType type, Guid userId, string? data, bool isRead, string navigateTo)
        {
            Title = title;
            Message = message;
            Type = type;
            UserId = userId;
            Data = data;
            IsRead = isRead;
            NavigateTo = navigateTo;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public Guid UserId { get; set; }
        public string? Data { get; set; }
        public bool IsRead { get; set; }
        public string NavigateTo { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }

        public void FromDomain(Domain.Notifications.SystemNotification domainEntity)
        {
            Title = domainEntity.Title;
            Message = domainEntity.Message;
            Type = domainEntity.Type;
            UserId = domainEntity.UserId;
            Data = domainEntity.Data;
            IsRead = domainEntity.IsRead;
            NavigateTo = domainEntity.NavigateTo;
        }

        public static Expression<Func<SystemNotification, Domain.Notifications.SystemNotification>> ToDomain =>
            notification => new Domain.Notifications.SystemNotification(
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.UserId,
                notification.Data,
                notification.IsRead,
                notification.NavigateTo
            )
            {
                CreatedAt = notification.CreatedAt.DateTime
            };

    }
}
