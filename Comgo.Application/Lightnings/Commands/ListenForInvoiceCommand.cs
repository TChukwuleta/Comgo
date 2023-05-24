using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Lightnings.Commands
{
    public class ListenForInvoiceCommand : IRequest<Result>
    {
    }

    public class ListenForInvoiceCommandHandler : IRequestHandler<ListenForInvoiceCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;
        public ListenForInvoiceCommandHandler(IAuthService authService, ILightningService lightningService)
        {
            _authService = authService;
            _lightningService = lightningService;
        }

        public async Task<Result> Handle(ListenForInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var listener = await _lightningService.ListenForSettledInvoice();
                if (listener == null)
                {
                    return Result.Failure("An error occured.");
                }

                var user = await _authService.GetUserByEmail(listener.Email);
                if (user.user == null)
                {
                    return Result.Failure("An error occured while confirming payment receipt. Invalid user details");
                }
                var updatePayment = await _authService.UpdateUserPaymentAsync(user.user, true);
                var paymentUpdateMessage = updatePayment.Message != null ? updatePayment.Message : updatePayment.Messages.FirstOrDefault();
                if (!updatePayment.Succeeded)
                {
                    return Result.Failure($"An error occured while confirming payment receipt. {paymentUpdateMessage}");
                }
                //var createCustody = await _bitcoinService.CreateNewKeyPair(user.user.UserId);
                return Result.Success("Invoice has been confirmed successfully.", listener);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Invoice confirmation was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
