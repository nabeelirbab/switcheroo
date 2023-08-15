using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILogger<GmailEmailSender> logger;

        public GmailEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<GmailEmailSender> logger)
        {
            this.smtpOptions = smtpOptions.Value;
            this.logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        private Task Execute(string email, string subject, string message)
        {
            if (!int.TryParse(smtpOptions.SMTP_PORT, out var port))
            {
                throw new InfrastructureException($"Invalid port {smtpOptions.SMTP_PORT}");
            }

            return Task.Run(() =>
            {
                using var client = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 465,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpOptions.SMTP_FROM_ADDRESS, smtpOptions.SMTP_PASSWORD)
                };

                var mailMessage = new MailMessage(smtpOptions.SMTP_FROM_ADDRESS, email, subject, message)
                {
                    IsBodyHtml = true
                };

                try
                {
                    client.Send(mailMessage);
                    logger.LogInformation("Email sent successfully to: {Email}", email);
                }
                catch (SmtpException ex)
                {
                    // Handle and log the exception
                    logger.LogError(ex, "Failed to send email to: {Email}", email);
                    throw;
                }
            });
        }
    }
}
