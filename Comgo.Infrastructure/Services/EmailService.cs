using Comgo.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<bool> SendEmailMessage(string body)
        {
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(username));
                email.To.Add(MailboxAddress.Parse(username));
                email.Subject = "New User Regstration";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };
                using var smtp = new SmtpClient();
                smtp.Connect(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(username, password);
                smtp.Send(email);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
