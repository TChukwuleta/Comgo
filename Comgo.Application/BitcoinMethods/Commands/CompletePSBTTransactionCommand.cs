using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Response;
using Comgo.Application.PSBTs.Commands;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class CompletePSBTTransactionCommand : IRequest<Result>
    {
        public string PSBT { get; set; }
        public string UserId { get; set; }
    }
    public class FinalizePSBTCommandHandler : IRequestHandler<CompletePSBTTransactionCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IBitcoinService _bitcoinService;
        private readonly IConfiguration _config;
        private readonly IAppDbContext _context;
        public FinalizePSBTCommandHandler(IAuthService authService, IBitcoinService bitcoinService, IAppDbContext context, IConfiguration config)
        {
            _authService = authService;
            _config = config;
            _bitcoinService = bitcoinService;
            _context = context;

        }
        public async Task<Result> Handle(CompletePSBTTransactionCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            var walletname = _config["Bitcoin:adminwallet"];
            PSBTResponse response = new PSBTResponse { 
                InitialPSBT = request.PSBT,
                UserSignedPSBT = request.PSBT
            };
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to finalize PSBT. Invalid user specified");
                }
                var psbtRecord = new CreatePSBTRecordCommand
                {
                    UserId = request.UserId,
                    InitialPSBT = request.PSBT,
                    UserSignedPSBT = request.PSBT,
                    Reference = reference,
                };
                var psbtRecordHandler = await new CreatePSBTRecordCommandHandler(_context).Handle(psbtRecord, cancellationToken);
                if (!psbtRecordHandler.Succeeded)
                {
                    return Result.Failure(psbtRecordHandler.Message);
                }
                var processPSBT = await _bitcoinService.ProcessPSBTAsync(walletname, request.PSBT);
                if (!processPSBT.success)
                {
                    return Result.Failure("An error occured while trying to finalize PSBT");
                }
                response.SystemSignedPSBT = processPSBT.message;
                var finalPsbt = processPSBT.message;
                if (!processPSBT.isComplete)
                {
                    var combinePSBT = await _bitcoinService.CombinePSBTAsync(walletname, request.PSBT, processPSBT.message);
                    if (!combinePSBT.success)
                    {
                        return Result.Failure($"An error occured. {combinePSBT.message}");
                    }
                    finalPsbt = combinePSBT.message;
                }
                var finalizePSBT = await _bitcoinService.FinalizePSBTAsync(walletname, finalPsbt);
                if (!finalizePSBT.success)
                {
                    return Result.Failure(finalizePSBT.message);
                }
                var broadcastTransaction = await _bitcoinService.BroadcastTransaction(walletname, finalizePSBT.message);
                if (!broadcastTransaction.success)
                {
                    return Result.Failure(broadcastTransaction.message);
                }
                return Result.Success("Transaction finalized successfully", broadcastTransaction.message);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unable to finalize PSBT: {ex.Message}");
            }
        }
    }
}
