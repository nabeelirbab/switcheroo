using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Feedback;
using Domain.Users;
using HotChocolate;
using System.Threading.Tasks;

namespace API.GraphQL.Feedback.Model
{
    public class Feedback
    {
        public Feedback(Guid? id, string title, string description, FeedbackStatus status, List<string>? attachments, Guid? createdByUserId, Guid? updatedByUserId)
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
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }


        public async Task<Users.Models.User> GetCreatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, CreatedByUserId.Value);
        }

        public async Task<Users.Models.User> GetUpdatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, UpdatedByUserId.Value);
        }
        private async Task<Users.Models.User> GetUserByUserId(IUserRepository userRepository, Guid userId)
        {
            var domUser = await userRepository.GetById(userId);

            if (domUser == null) throw new ApiException($"Invalid UserId {userId}");

            return Users.Models.User.FromDomain(domUser);
        }
        public static Feedback FromDomain(Domain.Feedback.Feedback domainFeedback)
        {
            if (!domainFeedback.Id.HasValue) throw new ApiException("Mapping error. Id missing");

            return new Feedback(
                domainFeedback.Id.Value,
                domainFeedback.Title,
                domainFeedback.Description,
                domainFeedback.Status,
                domainFeedback.Attachments,
                domainFeedback.CreatedByUserId.Value,
                domainFeedback.UpdatedByUserId.Value
                )
            {
                CreatedAt = domainFeedback.CreatedAt,
            };
        }
        public static List<Feedback> FromDomains(List<Domain.Feedback.Feedback> domainFeedbacks)
        {
            if (domainFeedbacks == null || domainFeedbacks.Count == 0)
            {
                return new List<Feedback>();
            }
            return domainFeedbacks.Select(newFeedback => new Feedback(
                newFeedback.Id,
                newFeedback.Title,
                newFeedback.Description,
                newFeedback.Status,
                newFeedback.Attachments,
                newFeedback.CreatedByUserId,
                newFeedback.UpdatedByUserId)
            {
                CreatedAt = newFeedback.CreatedAt
            })
                .ToList();
        }


    }
}
