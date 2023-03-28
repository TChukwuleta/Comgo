﻿using Comgo.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Net.Mail;
using System.Net;
using NBitcoin.Protocol;

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
