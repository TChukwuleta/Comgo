using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Comgo.Application.Users.Commands
{
    public class ValidatePaymentCommand : IRequest<Result>, IBaseValidator
    {
        public string OTP { get; set; }
        public string Reference { get; set; }
        public string UserId { get; set; }
    }

    public class ValidatePaymentCommandHandler : IRequestHandler<ValidatePaymentCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinService _bitcoinService;
        private readonly IConfiguration _config;
        public ValidatePaymentCommandHandler(IAuthService authService, IAppDbContext context, IBitcoinService bitcoinService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
            _context = context;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(ValidatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transaction creation failed. Invalid user details");
                }
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == request.Reference);
                if (transaction == null)
                {
                    return Result.Failure("No record found");
                }
                if (transaction.TransactionStatus != Core.Enums.TransactionStatus.Initiated)
                {
                    return Result.Failure($"This transaction has been {transaction.TransactionStatus.ToString()}");
                }
                var createPsbt = await _bitcoinService.CreateWalletPSTAsync(transaction.Amount, transaction.CreditAddress);
                if (!createPsbt.success)
                {
                    return Result.Failure("An error occured while generating PSBT for the transaction");
                }
                var walletname = _config["Bitcoin:adminwallet"];
                var processPsbt = await _bitcoinService.ProcessPSBTAsync(walletname, createPsbt.message.psbt.ToString());
                if (!processPsbt.success)
                {
                    return Result.Failure("An error occured while processing PSBT");
                }
                return Result.Success(processPsbt.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
