using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class ImportDescriptorCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class ImportDescriptorCommandHandler : IRequestHandler<ImportDescriptorCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        public ImportDescriptorCommandHandler(IBitcoinService bitcoinService, IAuthService authService, IConfiguration config)
        {
            _bitcoinService = bitcoinService;
            _authService = authService;
            _config = config;
        }
        public async Task<Result> Handle(ImportDescriptorCommand request, CancellationToken cancellationToken)
        {
            var adminDescriptor = _config["Bitcoin:AdminDescriptor"];
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Invalid user selected");
                }
                var importDescriptor = await _bitcoinService.ImportDescriptor(adminDescriptor, user.user);
                if (!importDescriptor.success)
                {
                    return Result.Failure(importDescriptor.message);
                }
                return Result.Success(importDescriptor.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Descriptor importation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
