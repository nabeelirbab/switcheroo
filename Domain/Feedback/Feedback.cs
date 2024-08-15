using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Feedback
{
    public enum FeedbackStatus
    {
        Pending = 0,
        Reviewed = 1
    }
    public class Feedback
    {
        public Feedback(Guid? id, string title, string description, FeedbackStatus status,List<string>? attachments, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            Title = title;
            Description = description;
            Status = status;
            Attachments = attachments;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        public string? Title { get; set; }
        [Required]
        public string Description { get; set; }
        public FeedbackStatus Status { get; set; }

        public List<string>? Attachments { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }


        public static Feedback CreateFeedback(
            string title,
            string description,
            FeedbackStatus status,
            List<string>? attachments,
            Guid createdByUserId
        )
        {
            return new Feedback(
                null,
                title,
                description,
                status,
                attachments,
                createdByUserId,
                createdByUserId
            )
            {
                CreatedAt = DateTime.Now
            };
        }

        public static Feedback UpdateFeedback(
            Guid id,
            string title,
            string description,
            FeedbackStatus status,
            List<string>? attachments,
            Guid updatedByUserId
        )
        {
            return new Feedback(
                id,
                title,
                description,
                status,
                attachments,
                updatedByUserId,
                updatedByUserId
            )
            {
                UpdatedAt = DateTime.Now
            };
        }
    }
}
