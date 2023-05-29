using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Queries
{
    public class GetUserByIdQuery : IRequest<Result>, IBaseValidator
    {
        public string UserId { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IEncryptionService _encryptionService;
        public GetUserByIdQueryHandler(IAuthService authService, IEncryptionService encryptionService)
        {
            _authService = authService;
            _encryptionService = encryptionService;
        }

        public async Task<Result> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetUserById(request.UserId);
                if (result.user == null)
                {
                    return Result.Failure("No user found");
                }
                if (!string.IsNullOrEmpty(result.user.Descriptor))
                {
                    result.user.Descriptor = _encryptionService.DecryptData(result.user.Descriptor);
                }
                return Result.Success(result.user);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Getting user by Id was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
