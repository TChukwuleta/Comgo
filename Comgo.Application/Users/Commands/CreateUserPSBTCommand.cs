using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class CreateUserPSBTCommand : IBaseValidator, IRequest<Result>
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string UserId { get; set; } 
    }

    public class CreateUserPSBTCommandHandler : IRequestHandler<CreateUserPSBTCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public CreateUserPSBTCommandHandler(IAuthService authService, IBitcoinService bitcoinService)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(CreateUserPSBTCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetUserById(request.UserId);
                if (result.user == null)
                {
                    return Result.Failure("No user found");
                }
                var generatedAddress = await _bitcoinService.CreatePSBTAsync(request.UserId, request.Address, request.Amount);
                if (!generatedAddress.success)
                {
                    return Result.Failure(generatedAddress.message);
                }
                return Result.Success("PSBT generated successfully", generatedAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"PSBT creation was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
