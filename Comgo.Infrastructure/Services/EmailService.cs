using Comgo.Application.Common.Interfaces;
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
        private readonly IAuthService _authService;
        private readonly EmailConfiguration _emailConfig;
        public EmailService(IConfiguration config, IAuthService authService, EmailConfiguration emailConfig)
        {
            _config = config;
            _authService = authService;
            _emailConfig = emailConfig;
        }

        
        public void SendEmail(Comgo.Application.Common.Interfaces.Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
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

        public async Task<bool> SendEmailMessage(string body, string subject, string recipient)
        {
            var username = _config["SMTP:Username"];
            var password = _config["SMTP:Password"];
            var host = _config["SMTP:Host"];
            var port = int.Parse(_config["SMTP:Port"]);
            try
            {
                var superAdmin = await _authService.GetSuperAdmin("");

                var client = new System.Net.Mail.SmtpClient(host, port)
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

        public async Task<bool> SendRegistrationEmailToUser(string Email, string message)
        {
            try
            {
                //var generateCode = GenerateCode(6);
                var sendUserEmail = new Comgo.Application.Common.Interfaces.Message(new string[] { Email }, "COMGO-REGISTRATION", "Hi, " + Email + ". Thank you for registering. Kindly complete your registration with the CODE: " + message);
                SendEmail(sendUserEmail);
                if (sendUserEmail == null)
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendConfirmationEmailToUser(string email, string firstname, string reference, string message)
        {
            try
            {
                //var generateCode = GenerateCode(6);
                var sendUserEmail = new Comgo.Application.Common.Interfaces.Message(new string[] { email }, "COMGO-REGISTRATION", "Hi, " + firstname + $". A transaction with transaction reference {reference} was initiated. Kindly use the OTP if you would like to proceed with this transaction: " + message);
                SendEmail(sendUserEmail);
                if (sendUserEmail == null)
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private MimeMessage CreateEmailMessage(Comgo.Application.Common.Interfaces.Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                    client.Send(mailMessage);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
