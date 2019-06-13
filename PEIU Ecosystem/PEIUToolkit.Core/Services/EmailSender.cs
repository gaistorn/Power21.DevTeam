using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PES.Toolkit.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfigOption _emailConfig;

        public EmailSender(EmailConfigOption emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_emailConfig.SmtpHost, _emailConfig.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailConfig.UserName, _emailConfig.Password),
                EnableSsl = _emailConfig.EnableSSL
            };

            client.Send(
                new MailMessage(_emailConfig.FromEmail, email, subject, htmlMessage) { IsBodyHtml = true }
                );
        }
    }
}
