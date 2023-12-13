using API.HtmlTemplates;
using Domain.Complaints;
using Domain.Items;
using Domain.Services;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Infrastructure.Email;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Mutation
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILoggerManager _loggerManager;

        public Mutation(IOptions<SmtpOptions> smtpOptions, ILoggerManager loggerManager)
        {
            this.smtpOptions = smtpOptions.Value;
            _loggerManager = loggerManager;
        }
        public async Task<Complaints.Models.Complaint> CreateUserComplaint(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            [Service] IComplaintRepository complaintRepository,
            [Service] IEmailSender emailSender,
            Complaint complaint,
            Guid userId
        )
        {
            try
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext == null) throw new ApiException("No httpcontext. Well isn't this just awkward?");

                var userCp = httpContextAccessor?.HttpContext?.User;

                if (userCp == null) throw new ApiException("Not authenticated");
                var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
                if (!user.Id.HasValue) throw new ApiException("Database failure");

                var newDomaincomplaint = await complaintRepository.CreateComplaintAsync(Complaint.CreateNewComplaint(
                    complaint.Title,
                    complaint.Description,
                    user.Id.Value
                ));

                var complaintUser = await userRepository.GetById(userId);

                var request = httpContext.Request;
                _loggerManager.LogError($"request {request}");
                var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
                _loggerManager.LogError($"basePath {basePath}");
                _loggerManager.LogError($"user.Email {user.Email}");
                var email = new ReportEmail(basePath, user.Email, complaintUser.Email, complaint.Title, complaint.Description);
                _loggerManager.LogError($"email {email}");
                await emailSender.SendEmailAsync(smtpOptions.SMTP_FROM_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());
                _loggerManager.LogError($"User from DB {smtpOptions.SMTP_FROM_SUPPORT_ADDRESS}");

                return Complaints.Models.Complaint.FromDomain(newDomaincomplaint);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }

        public async Task<Complaints.Models.Complaint> CreateItemComplaint(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            [Service] IUserRepository userRepository,
            [Service] IComplaintRepository complaintRepository,
            [Service] IEmailSender emailSender,
            Complaint complaint,
            Guid itemId
        )
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) throw new ApiException("No httpcontext. Well isn't this just awkward?");

            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var newDomaincomplaint = await complaintRepository.CreateComplaintAsync(Complaint.CreateNewComplaint(
                complaint.Title,
                complaint.Description,
                user.Id.Value
            ));

            var complaintItem = await itemRepository.GetItemByItemId(itemId);

            var complaintUser = await userRepository.GetById(complaintItem.CreatedByUserId);

            var request = httpContext.Request;
            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            var email = new ItemReport(basePath, user.Email, complaintUser.Email, complaint.Title, complaint.Description, itemId.ToString(), complaintItem.Title);
            await emailSender.SendEmailAsync(smtpOptions.SMTP_FROM_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());

            return Complaints.Models.Complaint.FromDomain(newDomaincomplaint);
        }
    }
}
