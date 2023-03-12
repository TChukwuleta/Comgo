using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators.UserValidator;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Users.Commands
{
    public class CreateUserCommand : IRequest<Result>, IUserRequestValidator
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public CreateUserCommandHandler(IAuthService authService, IAppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _authService = authService;
            _context = context;
            _config = config;
            _emailService = emailService;
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
                var allUsers = await _authService.GetAllUsers(0, 0);
                var existingUser = allUsers.users.FirstOrDefault(c => c.Name.ToLower() == request.Name.ToLower());
                if (existingUser != null)
                {
                    return Result.Failure("User already exist with this detail");
                }
                var newUser = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password,
                    UserType = Core.Enums.UserType.User

                };
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
                var sendEmail = await _emailService.SendEmailMessage(generateOtp.Entity.ToString(), "New User Regstration", request.Email);
                if (!sendEmail)
                {
                    return Result.Failure("An error occured while sending email");
                }
                return Result.Success("User creation was successful. Kindly check your email for token", newUser);
            }
            catch (Exception ex)
            {
                return Result.Failure("User creation failed", ex?.Message ?? ex?.InnerException.Message);
            }
        }
    }
}
