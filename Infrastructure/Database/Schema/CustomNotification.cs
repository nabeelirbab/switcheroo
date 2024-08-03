using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class CustomNotification : Audit
    {
        public CustomNotification(string title, string description)
        {
            Title = title;
            Description = description;
        }

        [Required]
        public Guid Id { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "The Title must be at most 500 characters long.")]
        public string Title { get; set; }
        [Required]
        [StringLength(3000, ErrorMessage = "The Description must be at most 3000 characters long.")]
        public string Description { get; set; }

        public void FromDomain(Domain.Notifications.CustomNotification domainNotification)
        {
            Title = domainNotification.Title;
            Description = domainNotification.Description;
        }

        public static Expression<Func<CustomNotification, Domain.Notifications.CustomNotification>> ToDomain =>
            notification => new Domain.Notifications.CustomNotification(
                notification.Id,
                notification.Title,
                notification.Description,
                notification.CreatedByUserId,
                notification.UpdatedByUserId
            )
            {
                CreatedAt = notification.CreatedAt.DateTime
            };

    }
}
