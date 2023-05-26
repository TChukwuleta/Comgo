using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Application.Transactions.Commands;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class InitiatePaymentCommand : IRequest<Result>, IBaseValidator
    {
        public decimal AmountInBtc { get; set; }
        public string Description { get; set; }
        public string DestinationAddress { get; set; }
        public string UserId { get; set; }
    }

    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinService _bitcoinService;
        public InitiatePaymentCommandHandler(IAuthService authService, IAppDbContext context, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _context = context;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transaction creation failed. Invalid user details");
                }
                var address = await _bitcoinService.GenerateDescriptorAddress(user.user.Descriptor);
                if (!address.success)
                {
                    return Result.Failure("Address generation was not successful");
                }
                var createTransactionRequest = new CreateTransactionCommand
                {
                    Description = request.Description,
                    Reference = reference,
                    CreditAddress = request.DestinationAddress,
                    DebitAddress = address.message,
                    PaymentMode = Core.Enums.PaymentModeType.Bitcoin,
                    Amount = request.AmountInBtc,
                    TransactionType = Core.Enums.TransactionType.Debit,
                    UserId = request.UserId
                };
                var confirmUserTransaction = await _bitcoinService.ValidateTransactionFromSender(request.UserId, reference, request.DestinationAddress);
                if (!confirmUserTransaction.success)
                {
                    return Result.Failure(confirmUserTransaction.message);
                }
                var handler = new TransactionHelper(_authService, _context);
                var createTransaction = await handler.CreateTransaction(createTransactionRequest, cancellationToken);
                if (createTransaction == null)
                {
                    return Result.Failure("An error occured while trying to create new transaction");
                }
                return Result.Success(confirmUserTransaction.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
