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
    public class SignSignatureCommand : IRequest<Result>, IBaseValidator
    {
        public string PublicKey { get; set; }
        public string UserId { get; set; }
    }

    public class SignSignatureCommandHandler : IRequestHandler<SignSignatureCommand, Result>
    {
        public Task<Result> Handle(SignSignatureCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
