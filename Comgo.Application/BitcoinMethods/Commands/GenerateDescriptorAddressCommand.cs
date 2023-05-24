using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class GenerateDescriptorAddressCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class GenerateDescriptorAddressCommandHandler : IRequestHandler<GenerateDescriptorAddressCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        private readonly IAuthService _authService;
        private readonly IEncryptionService _encryptionService;
        public GenerateDescriptorAddressCommandHandler(IBitcoinService bitcoinService, IAuthService authService, IEncryptionService encryptionService)
        {
            _bitcoinService = bitcoinService;
            _authService = authService;
            _encryptionService = encryptionService;
        }
        public async Task<Result> Handle(GenerateDescriptorAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string descriptor = default;
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate descriptor address. Invalid user specified");
                }
                if (string.IsNullOrEmpty(user.user.Descriptor))
                {
                    var importDescriptor = await _bitcoinService.ImportDescriptor(user.user);
                    if (!importDescriptor.success)
                    {
                        return Result.Failure(importDescriptor.message);
                    }
                    descriptor = importDescriptor.message;
                }
                else
                {
                    descriptor = _encryptionService.DecryptData(user.user.Descriptor);
                }
                var deriveAddress = await _bitcoinService.GenerateDescriptorAddress(descriptor);
                if (!deriveAddress.success)
                {
                    return Result.Failure(deriveAddress.message);
                }
                return Result.Success(deriveAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to generate descriptor address: {ex.Message}");
            }
        }
    }
}
