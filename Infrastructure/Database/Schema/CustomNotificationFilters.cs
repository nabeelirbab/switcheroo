using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class CustomNotificationFilters
    {
        public CustomNotificationFilters(Guid? customNotificationId, string? genderFilter, string? itemFilter)
        {
            CustomNotificationId = customNotificationId;
            GenderFilter = genderFilter;
            ItemFilter = itemFilter;
        }

        [Required]
        public Guid Id { get; set; }
        public CustomNotification CustomNotification { get; set; }
        [Required]
        [StringLength(500, ErrorMessage = "Custom Notification Id must be provided.")]
        [ForeignKey(nameof(CustomNotificationId))]
        public Guid? CustomNotificationId { get; set; }
        public string? GenderFilter { get; set; }
        public string? ItemFilter { get; set; }

        public void FromDomain(Domain.Notifications.CustomNotificationFilters domainFilters)
        {
            CustomNotificationId = domainFilters.CustomNotificationId;
            GenderFilter = domainFilters.GenderFilter;
            ItemFilter = domainFilters.ItemFilter;
        }

        public static Expression<Func<CustomNotificationFilters, Domain.Notifications.CustomNotificationFilters>> ToDomain =>
            notification => new Domain.Notifications.CustomNotificationFilters(
                notification.Id,
                notification.CustomNotificationId,
                notification.GenderFilter,
                notification.ItemFilter
            );

    }
}
