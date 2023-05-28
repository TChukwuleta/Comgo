using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class GetUserTokenByIdCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class GetUserTokenByIdCommandHandler : IRequestHandler<GetUserTokenByIdCommand, Result>
    {
        private readonly IAuthService _authService;
        public GetUserTokenByIdCommandHandler(IAuthService authService)
        {
            
        }
        public async Task<Result> Handle(GetUserTokenByIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user.Status != Core.Enums.Status.Active)
                {
                    return Result.Failure("User login was not successful. User is currently disabled");
                }
                var userLogin = await _authService.GetUserToken(user.user);
                if (userLogin == null)
                {
                    return Result.Failure($"User login was not successful.");
                }
                return Result.Success(userLogin);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User token retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}


