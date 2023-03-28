using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class GenerateMultisigAddressCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class GenerateMultisigAddressCommandHandler : IRequestHandler<GenerateMultisigAddressCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        private readonly IAuthService _authService;
        public GenerateMultisigAddressCommandHandler(IBitcoinService bitcoinService, IAuthService authService)
        {
            _bitcoinService = bitcoinService;
            _authService = authService;
        }

        public async Task<Result> Handle(GenerateMultisigAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetUserById(request.UserId);
                if (result.user == null)
                {
                    return Result.Failure("No user found");
                }

                var multisigAddress = await _bitcoinService.GenerateAddress(request.UserId);
                if (!multisigAddress.success)
                {
                    return Result.Failure(multisigAddress.message);
                }
                return Result.Success("Multisig address creation was successful", multisigAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Multisig address generation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
