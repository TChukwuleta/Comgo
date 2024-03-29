﻿using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators.UserValidator;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using NBitcoin;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace Comgo.Application.Users.Commands
{
    public class CreateUserCommand : IRequest<Result>, IUserRequestValidator
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Publickey { get; set; }
        public bool IsImportingKey { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IEncryptionService _encryptionService;
        private readonly Network _network;
        public CreateUserCommandHandler(IAuthService authService, IEmailService emailService, IEncryptionService encryptionService)
        {
            _authService = authService;
            _emailService = emailService;
            _network = Network.RegTest;
            _encryptionService = encryptionService;
        }

        public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user != null)
                {
                    return Result.Failure("User already exist with this detail");
                }
                MailAddress address = new MailAddress(request.Email);
                var allUsers = await _authService.GetAllUsers(0, 0);
                var existingUser = allUsers.users.FirstOrDefault(c => c.Name.ToLower() == request.Name.ToLower());
                if (existingUser != null)
                {
                    return Result.Failure("Kindly use a different name as a user with this name already exist");
                }
                var newUser = new User
                {
                    Name = request.Name,
                    Walletname = address.User,
                    Email = request.Email,
                    Password = request.Password,
                    UserType = Core.Enums.UserType.User
                };
                if (request.IsImportingKey)
                {
                    if (string.IsNullOrEmpty(request.Publickey))
                    {
                        return Result.Failure("Please input a public key or allow Comgo creates a new wallet for you");
                    }
                    var pubKey = ExtPubKey.Parse(request.Publickey, _network);
                    if (pubKey == null)
                    {
                        return Result.Failure("Invalid extended public key specified");
                    }
                    newUser.PublicKey = _encryptionService.EncryptData(request.Publickey);
                }
                var result = await _authService.CreateUserAsync(newUser);
                var error = string.IsNullOrEmpty(result.result.Message) ? result.result.Messages.FirstOrDefault() : result.result.Message;
                if (!result.result.Succeeded)
                {
                    return Result.Failure(error);
                }
                var generateOtp = await _authService.GenerateOTP(request.Email, "user-creation");
                if (!generateOtp.Succeeded)
                {
                    return Result.Failure("An error occured when generating otp");
                }
                var sendEmail = await _emailService.SendEmailAsync(request.Email, "Welcome on Board - Comgo", $"Hello {request.Name}, welcome to the cool kid's crib. Use this to complete your registration: {generateOtp.Entity.ToString()}");
                if (!sendEmail)
                {
                    return Result.Failure($"An error occured while sending email. OTP value is {generateOtp.Entity.ToString()}");
                }
                return Result.Success($"User creation was successful. Kindly check your email for token: {generateOtp.Entity.ToString()}", newUser);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User creation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
