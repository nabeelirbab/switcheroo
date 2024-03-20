using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Notifications
{
    public class CustomNotificationFilters
    {
        public CustomNotificationFilters(Guid? id, Guid? customNotificationId,string? genderFilter,string? itemFilter)
        {
            Id = id;
            CustomNotificationId=customNotificationId;
            GenderFilter=genderFilter;
            ItemFilter=itemFilter;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public Guid? CustomNotificationId { get; set; }

        public string? GenderFilter { get; set; }
        public string? ItemFilter { get; set; }


        public static CustomNotificationFilters CreateNewNotification(
            Guid? customNotificationId,
            string? genderFilter,
            string? itemFilter
        )
        {
            return new CustomNotificationFilters(
                null,
                customNotificationId,
                genderFilter,
                itemFilter

            );
        }
    }
}
