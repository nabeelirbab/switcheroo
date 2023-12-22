using API.HtmlTemplates;
using Domain.Complaints;
using Domain.Items;
using Domain.Services;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Infrastructure.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace API.GraphQL
{
    public partial class Mutation
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly ILoggerManager _loggerManager;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public Mutation(IHubContext<ChatHub> chatHubContext, IOptions<SmtpOptions> smtpOptions, ILoggerManager loggerManager)
        {
            _chatHubContext = chatHubContext;
            _smtpOptions = smtpOptions.Value;
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
                    user.Id.Value,
                    complaint.TargetUserId= userId,
                    null
                ));

                var complaintUser = await userRepository.GetById(userId);

                var request = httpContext.Request;
                var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
                var email = new ReportEmail(basePath, user.Email, complaintUser.Email, complaint.Title, complaint.Description);
                _loggerManager.LogError($"User from DB {_smtpOptions.SMTP_FROM_ADDRESS}");
                await emailSender.SendEmailAsync(_smtpOptions.SMTP_FROM_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());
                

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
                user.Id.Value,
                null,
                complaint.TargetItemId= itemId
            ));

            var complaintItem = await itemRepository.GetItemByItemId(itemId);

            var complaintUser = await userRepository.GetById(complaintItem.CreatedByUserId);

            var request = httpContext.Request;
            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            var email = new ItemReport(basePath, user.Email, complaintUser.Email, complaint.Title, complaint.Description, itemId.ToString(), complaintItem.Title);
            await emailSender.SendEmailAsync(_smtpOptions.SMTP_FROM_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());

            return Complaints.Models.Complaint.FromDomain(newDomaincomplaint);
        }
    }
}
