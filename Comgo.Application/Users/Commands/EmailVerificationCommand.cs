using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class EmailVerificationCommand : IRequest<Result>
    {
        public string Otp { get; set; }
        public string Email { get; set; }
    }

    public class EmailVerificationCommandHandler : IRequestHandler<EmailVerificationCommand, Result>
    {
        private readonly IAuthService _authService;
        public EmailVerificationCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result> Handle(EmailVerificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user == null)
                {
                    return Result.Failure("Error validating user. Invalid user details");
                }
                var confirmEmail = await _authService.EmailVerification(request.Email, request.Otp);
                var emailConfirmationResponse = confirmEmail.Message != null ? confirmEmail.Message : confirmEmail.Messages.FirstOrDefault();
                if (!confirmEmail.Succeeded)
                {
                    return Result.Failure(emailConfirmationResponse);
                }
                return Result.Success("User validation was successful. Kindly proceed to payment");
            }
            catch (Exception ex)
            {
                return Result.Failure($"User creation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}

