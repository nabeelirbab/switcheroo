using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;
using Domain.ContactUs;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<ContactUs.Model.ContactUs> CreateUserContactUs(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IContactUsRepository contactusRepository,
            [Service] IEmailSender emailSender,
            Domain.ContactUs.ContactUs contactUs
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

                var newDomaincontactus = await contactusRepository.CreateContactUsAsync(Domain.ContactUs.ContactUs.CreateNewContactUs(
                    contactUs.Title,
                    contactUs.Description,
                    contactUs.Name,
                    contactUs.Email,
                    user.Id.Value
                ));

                var request = httpContext.Request;
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
