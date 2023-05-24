using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class GeneratePSBTCommand : IRequest<Result>
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; }    
    }

    public class GeneratePSBTCommandHandler : IRequestHandler<GeneratePSBTCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        private readonly IAuthService _authService;
        public GeneratePSBTCommandHandler(IBitcoinService bitcoinService, IAuthService authService)
        {
            _bitcoinService = bitcoinService;
            _authService = authService;

        }
        public async Task<Result> Handle(GeneratePSBTCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate PSBT. Invalid user specified");
                }
                var deriveAddress = await _bitcoinService.CreateWalletPSTAsync(request.Amount, request.Address);
                if (!deriveAddress.success)
                {
                    return Result.Failure(deriveAddress.message);
                }
                return Result.Success(deriveAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to generate PSBT address: {ex.Message}");
            }
        }
    }
}
