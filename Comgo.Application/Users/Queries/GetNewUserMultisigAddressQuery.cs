using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        private readonly IAppDbContext _appDbContext;
        private readonly IEncryptionService _encryptionService;
        public GetNewUserMultisigAddressQueryHandler(IAuthService authService, IBitcoinService bitcoinService, IEncryptionService encryptionService, IAppDbContext appDbContext)
        {
            _authService = authService;
            _bitcoinService = bitcoinService;
            _encryptionService = encryptionService;
            _appDbContext = appDbContext;
        }

        public async Task<Result> Handle(GetNewUserMultisigAddressQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string descriptor = default;
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate descriptor address. Invalid user specified");
                }
                var userSettings = await _appDbContext.UserSettings.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userSettings == null || userSettings?.SecurityQuestionId <= 0)
                {
                    return Result.Failure("Kindly set up your security question and response to proceed");
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
                    descriptor = user.user.Descriptor;
                }
                var deriveAddress = await _bitcoinService.GenerateDescriptorAddress(descriptor);
                if (!deriveAddress.success)
                {
                    return Result.Failure(deriveAddress.message);
                }
                return Result.Success("Wallet address generated successfully", deriveAddress.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to generate descriptor address: {ex.Message}");
            }
        }
    }
}
