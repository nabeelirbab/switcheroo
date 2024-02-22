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
    public class ZohoEmailSender : IEmailSender
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILogger<ZohoEmailSender> logger;

        public ZohoEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<ZohoEmailSender> logger)
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
                    var sender = "switcheroo.app@hamzamfarooqi.com";
                    var senderTitle = "Switcheroo App";
                    var password = "rmnQ2?sk";
                    var smtpServerAddress = "smtp.zoho.com";
                    var smtpServerPort = 587;

                    using (var client = new SmtpClient(smtpServerAddress, smtpServerPort))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(sender, password);
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
                    Console.WriteLine($"Error => {e.Message}");
                }
            });

        }
    }
}
