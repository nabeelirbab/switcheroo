using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email
{
    public class DevEmailSender : IEmailSender
    {
        private readonly SmtpOptions smtpOptions;

        public DevEmailSender(IOptions<SmtpOptions> smtpOptions)
        {
            this.smtpOptions = smtpOptions.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(
              subject,
              htmlMessage,
              email
            );
        }

        private Task Execute(string subject, string message, string email)
        {
            if (!int.TryParse(smtpOptions.SMTP_PORT, out var port))
            {
                throw new InfrastructureException($"Invalid port {smtpOptions.SMTP_PORT}");
            }

            return Task.Run(() =>
            {
                using var client = new SmtpClient(smtpOptions.SMTP_HOST, port);
                var mailMessage = new MailMessage(smtpOptions.SMTP_FROM_ADDRESS, email, subject, message)
                {
                    IsBodyHtml = true
                };

                client.Send(mailMessage);
            });
        }
    }
}
