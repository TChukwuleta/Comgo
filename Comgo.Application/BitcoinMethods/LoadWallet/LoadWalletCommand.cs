using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.BitcoinMethods.LoadWallet
{
    public class LoadWalletCommand : IRequest<Result>
    {
        public string WalletName { get; set; }
        public string MethodName { get; set; }
    }

    public class LoadWalletCommandHandler : IRequestHandler<LoadWalletCommand, Result>
    {
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public LoadWalletCommandHandler(IBitcoinCoreClient bitcoinCoreClient)
        {
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(LoadWalletCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var loadwallet = await _bitcoinCoreClient.BitcoinRequestServer(request.MethodName);
                if (string.IsNullOrEmpty(loadwallet))
                {
                    return Result.Failure("An error occured while loading wallet");
                }
                return Result.Success(loadwallet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
