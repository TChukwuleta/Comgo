using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users
{
    public class CreateDescriptorStringCommand : IRequest<Result>
    {
        public string UserId { get; set; }
    }

    public class CreateDescriptorStringCommandHandler : IRequestHandler<CreateDescriptorStringCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        private readonly IAuthService _authService;
        public CreateDescriptorStringCommandHandler(IBitcoinService bitcoinService, IAuthService authService)
        {
            _bitcoinService = bitcoinService;
            _authService = authService;
        }
        public async Task<Result> Handle(CreateDescriptorStringCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Invalid user selected");
                }
                var response = await _bitcoinService.GenerateDescriptor(user.user);
                if (!response.success)
                {
                    return Result.Failure($"Failed to retrieve wallet descriptors. {response.message}");
                }
                return Result.Success(response.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Descriptor generation failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
