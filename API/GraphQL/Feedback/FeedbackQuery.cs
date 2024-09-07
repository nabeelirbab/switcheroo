using API.GraphQL.ContactUs.Model;
using Domain.Complaints;
using Domain.Feedback;
using Domain.Notifications;
using GraphQL;
using HotChocolate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<Feedback.Model.Feedback>> GetFeedbacks(
            [Service] IFeedbackRepository feedbackRepository)
        {
            var feedbacks = await feedbackRepository.GetFeedbacks();
            return Feedback.Model.Feedback.FromDomains(feedbacks);
        }
        public async Task<List<Feedback.Model.Feedback>> GetFeedbacksByStatus(
            [Service] IFeedbackRepository feedbackRepository,
            FeedbackStatus status)
        {
            var feedbacks = await feedbackRepository.GetFeedbacksByStatus(status);
            return Feedback.Model.Feedback.FromDomains(feedbacks);
        }
    }
}
