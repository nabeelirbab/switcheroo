using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<SendGridEmailSender> logger;
        public SendGridEmailSender(IOptions<SmtpOptions> options, ILogger<SendGridEmailSender> logger)
        {
            _smtpOptions = options.Value;
            this.logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.Run(async () =>
            {
                var client = new SendGridClient(_smtpOptions.EMAIL_API_KEY);
                var from = new EmailAddress(_smtpOptions.SMTP_FROM_ADDRESS, "No Reply");
                var to = new EmailAddress(email);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, htmlMessage, htmlMessage);
                try
                {
                    await client.SendEmailAsync(msg);
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