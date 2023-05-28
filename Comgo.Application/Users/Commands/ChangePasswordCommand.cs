using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class ChangePasswordCommand : IRequest<Result>
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IAuthService _authService;
        public ChangePasswordCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _authService.GetUserById(request.UserId);
                if (existingUser.user == null)
                {
                    return Result.Failure("Password change was not successful. Invalid user details");
                }
                return await _authService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Password change was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
