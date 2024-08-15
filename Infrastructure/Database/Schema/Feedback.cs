using Domain.Feedback;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class Feedback : Audit
    {
        public Feedback(string title, string description, FeedbackStatus status, List<string>? attachments)
        {
            Title = title;
            Description = description;
            Status = status;
            Attachments = attachments;
        }

        [Required]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        [Required]
        [StringLength(3000, ErrorMessage = "The Description must be at most 3000 characters long.")]
        public string Description { get; set; }

        public FeedbackStatus Status { get; set; }

        public List<string>? Attachments { get; set; }

        public void FromDomain(Domain.Feedback.Feedback domainFeedback)
        {
            Title = domainFeedback.Title;
            Description = domainFeedback.Description;
            Status = domainFeedback.Status;
            Attachments = domainFeedback.Attachments;
        }

        public static Expression<Func<Feedback, Domain.Feedback.Feedback>> ToDomain =>
            feedback => new Domain.Feedback.Feedback(
                feedback.Id,
                feedback.Title,
                feedback.Description,
                feedback.Status,
                feedback.Attachments,
                feedback.CreatedByUserId,
                feedback.UpdatedByUserId
            )
            {
                CreatedAt = feedback.CreatedAt.DateTime
            };

    }
}
