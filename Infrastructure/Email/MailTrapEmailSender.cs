using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email
{
    public class MailTrapEmailSender : IEmailSender
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILogger<MailTrapEmailSender> logger;

        public MailTrapEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<MailTrapEmailSender> logger)
        {
            this.smtpOptions = smtpOptions.Value;
            this.logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(
             email,
             subject,
             htmlMessage
           );
        }

        private Task Execute(string email, string subject, string htmlContent)
        {
            return Task.Run(() =>
            {
                try
                {
                    var sender = "info@switcherooapp.com";
                    var senderTitle = "Switcheroo App";
                    var password = "1272b84979a3f9aefde975bc28510125";
                    var smtpServerAddress = "live.smtp.mailtrap.io";
                    var smtpServerPort = 587;

                    using (var client = new SmtpClient(smtpServerAddress, smtpServerPort))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential("api", password);
                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(sender, senderTitle),
                            Subject = subject,
                            Body = htmlContent,
                            IsBodyHtml = true,
                        };
                        mailMessage.To.Add(email);
                        client.Send(mailMessage);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Error => {e.Message}");
                }
            });
        }
    }
}
