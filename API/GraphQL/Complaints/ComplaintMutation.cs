using API.GraphQL.CommonServices;
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Complaints.Models.Complaint> CreateUserComplaint(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            [Service] IComplaintRepository complaintRepository,
            [Service] IEmailSender emailSender,
            Complaint complaint,
            Guid userId
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var requestUserEmail = userContextService.GetCurrentUserEmail();
                var newDomaincomplaint = await complaintRepository.CreateComplaintAsync(Complaint.CreateNewComplaint(
                    complaint.Title,
                    complaint.Description,
                    requestUserId,
                    complaint.TargetUserId = userId,
                    null
                ));

                var complaintUser = await userRepository.GetById(userId);

                var request = userContextService.GetHttpRequestContext().Request;
                var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
                var email = new ReportEmail(basePath, requestUserEmail, complaintUser.Email, complaint.Title, complaint.Description);
                await emailSender.SendEmailAsync(_smtpOptions.SMTP_USER_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());


                return Complaints.Models.Complaint.FromDomain(newDomaincomplaint);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Complaints.Models.Complaint> CreateItemComplaint(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            [Service] IUserRepository userRepository,
            [Service] IComplaintRepository complaintRepository,
            [Service] IEmailSender emailSender,
            Complaint complaint,
            Guid itemId
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var requestUserEmail = userContextService.GetCurrentUserEmail();
            var newDomaincomplaint = await complaintRepository.CreateComplaintAsync(Complaint.CreateNewComplaint(
                complaint.Title,
                complaint.Description,
                requestUserId,
                null,
                complaint.TargetItemId = itemId
            ));

            var complaintItem = await itemRepository.GetItemByItemId(itemId);

            var complaintUser = await userRepository.GetById(complaintItem.CreatedByUserId);

            var request = userContextService.GetHttpRequestContext().Request;
            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
            var email = new ItemReport(basePath, requestUserEmail, complaintUser.Email, complaint.Title, complaint.Description, itemId.ToString(), complaintItem.Title);
            await emailSender.SendEmailAsync(_smtpOptions.SMTP_ITEM_SUPPORT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());

            return Complaints.Models.Complaint.FromDomain(newDomaincomplaint);
        }
    }
}
