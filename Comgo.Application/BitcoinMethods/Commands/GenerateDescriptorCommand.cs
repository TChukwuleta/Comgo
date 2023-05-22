using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.BitcoinMethods.Commands
{
    public class GenerateDescriptorCommand : IRequest<Result>
    {
        public string KeyOne { get; set; }
        public string KeyTwo { get; set; }
    }

    public class GenerateDescriptorCommandHandler : IRequestHandler<GenerateDescriptorCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        public GenerateDescriptorCommandHandler(IBitcoinService bitcoinService)
        {
            _bitcoinService = bitcoinService;
        }
        public async Task<Result> Handle(GenerateDescriptorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var descriptors = await _bitcoinService.CreateDescriptorString(request.KeyOne, request.KeyTwo);
                return Result.Success(descriptors.message);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
