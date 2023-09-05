using System;
using System.Threading.Tasks;
using API.HtmlTemplates;
using HotChocolate;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace API.GraphQL;

public partial class Mutation
{
    public async Task<bool> TestEmail(
        [Service] IEmailSender emailSender
        )
    {
        try
        {
            var basePath = "";
            var email = "h.halai0334@gmail.com";
            var emailConfirmCode = "00000";
            var emailBody = new SignUpConfirmationEmail(basePath, emailConfirmCode, email, $"Hello World");
            await emailSender.SendEmailAsync(email, "Switcheroo Email Confirmation", emailBody.GetHtmlString());
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}