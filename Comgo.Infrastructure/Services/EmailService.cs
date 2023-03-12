using Comgo.Application.Common.Interfaces;
//using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Comgo.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        public EmailService(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }
        public async Task<bool> SendEmailMessage(string body, string subject, string recipient)
        {
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            try
            {
                var superAdmin = await _authService.GetSuperAdmin("");

                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                client.Send(superAdmin.user.Email, recipient, subject, body);
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
