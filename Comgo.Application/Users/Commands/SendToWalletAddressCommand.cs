using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class SendToWalletAddressCommand : IRequest<Result>, IBaseValidator
    {
        public string Amount { get; set; }
        public string DestinationAddress { get; set; }
        public string UserId { get; set; }
    }

    public class SendToWalletAddressCommandHandler : IRequestHandler<SendToWalletAddressCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinService _bitcoinService;
        public SendToWalletAddressCommandHandler(IAuthService authService, IAppDbContext context, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _context = context;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(SendToWalletAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transaction creation failed. Invalid user details");
                }
                var sendPayment = await _bitcoinService.SendToAddress(request.DestinationAddress, request.Amount);
                if (!sendPayment.success)
                {
                    return Result.Failure("Unable to send payment to address");
                }
                return Result.Success(sendPayment.message);

            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
