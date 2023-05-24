using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Entities;
using Comgo.Core.Enums;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Transactions.Commands
{
    public class CreateTransactionCommand : IRequest<Result>, IBaseValidator
    {
        public string Description { get; set; }
        public string Reference { get; set; }
        public string DebitAddress { get; set; }
        public string CreditAddress { get; set; }
        public PaymentModeType PaymentMode { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string UserId { get; set; }
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;

        public CreateTransactionCommandHandler(IAuthService authService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<Result> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transaction creation failed. Invalid user details");
                }

                var entity = new Transaction
                {
                    DebitAddress = request.DebitAddress,
                    CreditAddress = request.CreditAddress,
                    UserId = request.UserId,
                    Amount = request.Amount,
                    TransactionType = request.TransactionType,
                    TransactionReference = reference,
                    PaymentModeType = request.PaymentMode,
                    TransactionStatus = TransactionStatus.Initiated,
                    Narration = request.Description,
                    CreatedDate = DateTime.Now
                };
                await _context.Transactions.AddAsync(entity);

                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Transaction creation was successful", entity);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Transactions creation was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
