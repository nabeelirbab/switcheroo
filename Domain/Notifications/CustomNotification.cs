using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Notifications
{
    public class CustomNotification
    {
        public CustomNotification(Guid? id, string title, string description, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            Title = title;
            Description = description;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public static CustomNotification CreateNewNotification(
            string title,
            string description,
            Guid createdByUserId
        )
        {
            return new CustomNotification(
                null,
                title,
                description,
                createdByUserId,
                createdByUserId
            )
            {
                CreatedAt = DateTime.Now
            };
        }

        public static CustomNotification CreateUpdateNotification(
            Guid id,
            string title,
            string description,
            Guid updatedByUserId
        )
        {
            return new CustomNotification(
                id,
                title,
                description,
                updatedByUserId,
                updatedByUserId
            )
            {
                UpdatedAt = DateTime.Now
            };
        }
    }
}
