using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendWithBrevo;

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

        private async Task Execute(string email, string subject, string message)
        {
            //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

            BrevoClient client = new BrevoClient("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");
            var result = await client.SendAsync(
                new Sender("Switchhero", "switchherodev@gmail.com"),
                new List<Recipient> { new Recipient("User", email) },
                subject,
                message,
                true // true if body is HTML
            );
            Console.WriteLine(result);
            // if (!int.TryParse(smtpOptions.SMTP_PORT, out var port))
            // {
            //     throw new InfrastructureException($"Invalid port {smtpOptions.SMTP_PORT}");
            // }
            // using var client = new SmtpClient()
            // {
            //     Host = smtpOptions.SMTP_HOST,
            //     Port = int.Parse(smtpOptions.SMTP_PORT),
            //     EnableSsl = false,
            //     Credentials = new NetworkCredential(smtpOptions.SMTP_FROM_ADDRESS, smtpOptions.SMTP_PASSWORD)
            // };
            //
            // var mailMessage = new MailMessage(smtpOptions.SMTP_FROM_ADDRESS, email, subject, message)
            // {
            //     IsBodyHtml = true
            // };
            //
            // try
            // {
            //     client.Send(mailMessage);
            //     logger.LogInformation("Email sent successfully to: {Email}", email);
            // }
            // catch (SmtpException ex)
            // {
            //     // Handle and log the exception
            //     logger.LogError(ex, "Failed to send email to: {Email}", email);
            // }
            // return Task.CompletedTask;
        }
    }
}
