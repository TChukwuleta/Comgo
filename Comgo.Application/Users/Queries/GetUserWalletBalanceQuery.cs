using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Queries
{
    public class GetUserWalletBalanceQuery : IBaseValidator, IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class GetUserWalletBalanceQueryHandler : IRequestHandler<GetUserWalletBalanceQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public GetUserWalletBalanceQueryHandler(IAuthService authService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(GetUserWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetUserById(request.UserId);
                if (result.user == null)
                {
                    return Result.Failure("No user found");
                }
                var generatedAddress = await _bitcoinService.GetWalletBalance(request.UserId);
                if (!generatedAddress.success)
                {
                    return Result.Failure("An error occured while trying to retrieve user balance. Please contact support");
                }
                return Result.Success("User wallet balance retrieved successfully", generatedAddress.response);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User wallet balance retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
