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
        public string Parameter { get; set; }
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
                var pubkeyOne = "03789ed0bb717d88f7d321a368d905e7430207ebbd82bd342cf11ae157a7ace5fd";
                var pubKeyTwo = "03dbc6764b8884a92e871274b87583e6d5c2a58819473e17e107ef3f6aa5a61626";
                var keysList = new List<string> { pubkeyOne, pubKeyTwo };

                /*var multisig = await _bitcoinCoreClient.BitcoinRequestServer(request.MethodName, keysList, 2);
                if (!string.IsNullOrEmpty(multisig))
                {
                    return Result.Success(multisig);
                }
                var loadwallet = await _bitcoinCoreClient.BitcoinRequestServer(request.MethodName);
                if (string.IsNullOrEmpty(loadwallet))
                {
                    return Result.Failure("An error occured while loading wallet");
                }*/
                var walletInfo = await _bitcoinCoreClient.WalletInformation(request.WalletName, request.MethodName);
                if (string.IsNullOrEmpty(walletInfo))
                {
                    return Result.Failure("AN error occured");
                }
                return Result.Success(walletInfo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
