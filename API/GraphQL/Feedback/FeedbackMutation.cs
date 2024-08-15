using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;
using Domain.Feedback;
using API.GraphQL.CommonServices;
using Amazon.S3.Model;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Feedback.Model.Feedback> CreateFeedback(
            [Service] UserContextService userContextService,
            [Service] IFeedbackRepository feedbackRepository,
            Domain.Feedback.Feedback feedback
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var newDomainFeedback = await feedbackRepository.CreateFeedbackAsync(Domain.Feedback.Feedback.CreateFeedback(
                    feedback.Title,
                    feedback.Description,
                    feedback.Status,
                    feedback.Attachments,
                    Guid.NewGuid()
                ), requestUserId);
                return Feedback.Model.Feedback.FromDomain(newDomainFeedback);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }
        public async Task<string> UpdateFeedbackStatus(
            [Service] UserContextService userContextService,
            [Service] IFeedbackRepository feedbackRepository,
            Guid id,
            FeedbackStatus status
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                return await feedbackRepository.UpdateFeedbackStatusAsync(id, requestUserId, status);

            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }
    }
}
