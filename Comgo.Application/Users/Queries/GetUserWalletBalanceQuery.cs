using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        private readonly IAppDbContext _context;
        public GetUserWalletBalanceQueryHandler(IAuthService authService, IBitcoinService bitcoinService, IAppDbContext context)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
            _context = context;
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
                var userSettings = await _context.UserSettings.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userSettings == null || userSettings?.SecurityQuestionId <= 0)
                {
                    return Result.Failure("Kindly set up your security question and response to proceed");
                }
                var walletBalance = await _bitcoinService.GetDescriptorBalance(result.user.Descriptor);
                if (!walletBalance.success)
                {
                    return Result.Failure("An error occured while trying to retrieve user balance. Please contact support");
                }
                return Result.Success("User wallet balance retrieved successfully", $"{walletBalance.amount} BTC");
            }
            catch (Exception ex)
            {
                return Result.Failure($"User wallet balance retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
