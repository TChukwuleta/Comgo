using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Users
{
    public class CreateDescriptorStringCommand : IRequest<Result>
    {
        public string PubKeyOne { get; set; }
        public string PubKeyTwo { get; set; }
    }

    public class CreateDescriptorStringCommandHandler : IRequestHandler<CreateDescriptorStringCommand, Result>
    {
        private readonly IBitcoinService _bitcoinService;
        public CreateDescriptorStringCommandHandler(IBitcoinService bitcoinService)
        {
            _bitcoinService = bitcoinService;
        }
        public async Task<Result> Handle(CreateDescriptorStringCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _bitcoinService.CreateDescriptorString(request.PubKeyOne, request.PubKeyTwo);
                return Result.Success(response.message);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
