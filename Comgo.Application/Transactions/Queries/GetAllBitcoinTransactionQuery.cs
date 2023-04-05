using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.Transactions.Queries
{
    public class GetAllBitcoinTransactionQuery : IRequest<Result>, IBaseValidator
    {
        public string UserId { get; set; }
    }

    public class GetAllBitcoinTransactionQueryHandler : IRequestHandler<GetAllBitcoinTransactionQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinService _bitcoinService;

        public GetAllBitcoinTransactionQueryHandler(IAuthService authService, IAppDbContext context, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _context = context;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(GetAllBitcoinTransactionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transactions retrieval was not successful. Invalid user details specified");
                }
                var transactions = await _bitcoinService.ListTransactions(request.UserId);
                if (!transactions.success)
                {
                    return Result.Failure("Unable to retrieve bitcoin transactiton. Please contact support");
                }
                return Result.Success("Transactions retrieval was successful", transactions);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User bitcoin transactions retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
