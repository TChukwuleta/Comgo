using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Paystacks.Commands
{
    public class VerifyPaystackCommand : IRequest<Result>
    {
        public string Reference { get; set; }
        public string Email { get; set; }
    }

    public class VerifyPaystackCommandHandler : IRequestHandler<VerifyPaystackCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IPaystackService _paystackService;
        private readonly IBitcoinService _bitcoinService;

        public VerifyPaystackCommandHandler(IAuthService authService, IPaystackService paystackService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
            _paystackService = paystackService;
        }

        public async Task<Result> Handle(VerifyPaystackCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user == null)
                {
                    return Result.Failure("Unable to make payment. Invalid user details");
                }
                var verifyPayment = await _paystackService.VerifyPayment(request.Reference);
                if (!verifyPayment)
                {
                    return Result.Failure("Transaction verification was not successful");
                }
                var verifyUserPayment = await _authService.UpdateUserPaymentAsync(user.user, true);
                var verifyPaymentResponse = verifyUserPayment.Message != null ? verifyUserPayment.Message : verifyUserPayment.Messages.FirstOrDefault();
                if (!verifyUserPayment.Succeeded)
                {
                    return Result.Failure(verifyPaymentResponse);
                }

                //var createCustody = await _bitcoinService.CreateNewKeyPair(user.user.UserId);
                return Result.Success("Payment verification was successful. User can now proceed with this application");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Service payment failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
