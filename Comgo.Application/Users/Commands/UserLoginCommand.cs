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
                var userLogin = await _authService.Login(request.Email, request.Password);
                if (userLogin == null)
                {
                    return Result.Failure($"User login was not successful.");
                }
                var createCustody = await _bitcoinService.CreateNewKeyPairAsync(userLogin.UserId);
                var response = new
                {
                    LoginDetails = userLogin,
                    Signature = createCustody.entity
                };
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User login was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
