using Comgo.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace Comgo.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }


        public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var key = _config["SendGrid:ApiKey"];
            var name = _config["SendGrid:UserName"];
            var fromAddress = _config["SendGrid:From"];
            try
            {
                var sendGridClient = new SendGridClient(key);
                var from = new EmailAddress(fromAddress, name);
                var to = new EmailAddress(email);
                var plainTextContent = Regex.Replace(htmlMessage, "<[^>]*>", "");
                var msg = MailHelper.CreateSingleEmail(from, to, subject,
                plainTextContent, htmlMessage);
                var response = await sendGridClient.SendEmailAsync(msg);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendEmailViaGmailAsync(string body, string otp, string subject, string recipientEmail, string recipientName)
        {
            var server = _config["EmailConfiguration:SmtpServer"];
            var username = _config["EmailConfiguration:Username"];
            var password = _config["EmailConfiguration:Password"];
            var host = _config["EmailConfiguration:Host"];
            int.TryParse(_config["EmailConfiguration:Port"], out int port);
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Comgo App", username));
                email.To.Add(new MailboxAddress(recipientName, recipientEmail));
                email.Subject = subject;
                if (!string.IsNullOrEmpty(otp))
                {
                    email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = $"<b>We are glad you joined us. Kindly use this OTP to proceed: {otp}</b>"
                    };
                }
                else
                {
                    email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = $"<b>{body}</b>"
                    };
                }
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(server, 587, false); // Port can also be 587
                    smtp.Authenticate(username, password);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
