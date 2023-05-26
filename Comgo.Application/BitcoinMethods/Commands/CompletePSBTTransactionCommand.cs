using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class CompletePSBTTransactionCommand : IRequest<Result>
    {
        public string PSBT { get; set; }
        public string UserId { get; set; }
    }
    public class FinalizePSBTCommandHandler : IRequestHandler<CompletePSBTTransactionCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public FinalizePSBTCommandHandler(IAuthService authService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
        }
        public async Task<Result> Handle(CompletePSBTTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to finalize PSBT. Invalid user specified");
                }
                var deriveAddress = await _bitcoinService.ProcessPSBTAsync(user.user.Walletname, request.PSBT);
                if (!deriveAddress.success)
                {
                    return Result.Failure("An error occured while trying to finalize PSBT");
                }
                return Result.Success(deriveAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to finalize PSBT: {ex.Message}");
            }
        }
    }
}
