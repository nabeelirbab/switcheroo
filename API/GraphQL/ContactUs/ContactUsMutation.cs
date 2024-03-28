using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;
using Domain.ContactUs;
using API.GraphQL.CommonServices;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<ContactUs.Model.ContactUs> CreateUserContactUs(
            [Service] UserContextService userContextService,
            [Service] IContactUsRepository contactusRepository,
            [Service] IEmailSender emailSender,
            Domain.ContactUs.ContactUs contactUs
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var newDomaincontactus = await contactusRepository.CreateContactUsAsync(Domain.ContactUs.ContactUs.CreateNewContactUs(
                    contactUs.Title,
                    contactUs.Description,
                    contactUs.Name,
                    contactUs.Email,
                    requestUserId
                ));

                var request = userContextService.GetHttpRequestContext().Request;
                var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";
                var email = new ContactUsEmail(basePath, contactUs.Email, contactUs.Title, contactUs.Description, contactUs.Name);
                await emailSender.SendEmailAsync(_smtpOptions.SMTP_CONTACT_ADDRESS, "Switcheroo Complaint Email", email.GetHtmlString());
                return ContactUs.Model.ContactUs.FromDomain(newDomaincontactus);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"API Exception {ex}");
            }
        }
    }
}
