using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators.UserValidator;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using System.Net.Mail;

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
        private readonly IEmailService _emailService;
        public CreateUserCommandHandler(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
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
                var sendEmail = await _emailService.SendEmailViaGmailAsync("", generateOtp.Entity.ToString(), "Welcome on Board", request.Email, request.Name);
                if (!sendEmail)
                {
                    return Result.Failure("An error occured while sending email");
                }
                return Result.Success("User creation was successful. Kindly check your email for token", newUser);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User creation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
