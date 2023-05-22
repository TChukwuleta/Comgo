using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators.UserValidator;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class UserLoginCommand : IRequest<Result>, IUserLoginValidator
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public UserLoginCommandHandler(IAuthService authService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                var userLogin = await _authService.Login(request.Email, request.Password);
                if (userLogin == null)
                {
                    return Result.Failure($"User login was not successful.");
                }
                if (!user.user.IsWalletCreated)
                {
                    var createWallet = await _bitcoinService.CreateUserWallet(user.user);
                    if (!createWallet.success)
                    {
                        return Result.Failure(createWallet.message);
                    }
                }
                return Result.Success(userLogin);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User login was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
