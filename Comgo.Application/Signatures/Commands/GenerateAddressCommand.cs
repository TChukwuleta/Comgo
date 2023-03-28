using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Signatures.Commands
{
    public class GenerateAddressCommand : IRequest<Result>, IBaseValidator
    {
        public string UserId { get; set; }
    }

    public class GenerateAddressCommandHandler : IRequestHandler<GenerateAddressCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        public GenerateAddressCommandHandler(IAppDbContext context, IAuthService authService, IBitcoinService bitcoinService)
        {
            _context = context;
            _authService = authService;
            _bitcoinService = bitcoinService;
        }

        public async Task<Result> Handle(GenerateAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate new address. Invalid user details");
                }
                return Result.Success("Tru");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
