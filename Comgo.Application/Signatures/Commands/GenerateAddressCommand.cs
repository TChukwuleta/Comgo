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
        public string PublicKey { get; set; }
        public string UserId { get; set; }
    }

    public class GenerateAddressCommandHandler : IRequestHandler<GenerateAddressCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public Task<Result> Handle(GenerateAddressCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
