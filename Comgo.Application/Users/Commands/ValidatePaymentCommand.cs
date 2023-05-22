using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        private readonly IEmailService _emailService;
        public ValidatePaymentCommandHandler(IAuthService authService, IAppDbContext context, IBitcoinService bitcoinService, IEmailService emailService)
        {
            _authService = authService;
            _context = context;
            _bitcoinService = bitcoinService;
            _emailService = emailService;
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
                return Result.Success("done");

            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
