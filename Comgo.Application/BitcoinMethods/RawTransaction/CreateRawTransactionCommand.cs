using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.BitcoinMethods.RawTransaction
{
    public class CreateRawTransactionCommand : IRequest<Result>
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public decimal MyProperty { get; set; }
    }

    public class CreateRawTransactionCommandHandler : IRequestHandler<CreateRawTransactionCommand, Result>
    {
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public CreateRawTransactionCommandHandler(IBitcoinCoreClient bitcoinCoreClient)
        {
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public Task<Result> Handle(CreateRawTransactionCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
