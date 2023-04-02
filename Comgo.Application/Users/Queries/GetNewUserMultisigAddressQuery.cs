using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Queries
{
    public class GetNewUserMultisigAddressQuery : IBaseValidator, IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class GetNewUserMultisigAddressQueryHandler : IRequestHandler<GetNewUserMultisigAddressQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public GetNewUserMultisigAddressQueryHandler(IAuthService authService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(GetNewUserMultisigAddressQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetUserById(request.UserId);
                if (result.user == null)
                {
                    return Result.Failure("No user found");
                }
                var generatedAddress = await _bitcoinService.GenerateAddress(request.UserId);
                if (!generatedAddress.success)
                {
                    return Result.Failure(generatedAddress.message);
                }
                return Result.Success("Multisig address generated successfully", generatedAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Address generation was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
