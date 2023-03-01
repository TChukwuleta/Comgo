using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Application.Common.Model.Request;
using Comgo.Core.Enums;
using Comgo.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Users.Commands
{
    public class PaymentServiceCommand : IRequest<Result>, IEmailValidator
    {
        public string Email { get; set; }
        public PaymentModeType PaymentModeType { get; set; }
    }

    public class PaymentServiceCommandHandler : IRequestHandler<PaymentServiceCommand, Result>
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;
        private readonly IPaystackService _paystackService;
        public PaymentServiceCommandHandler(IConfiguration config, IAuthService authService, IPaystackService paystackService)
        {
            _config = config;
            _authService = authService;
            _paystackService = paystackService;
        }

        public async Task<Result> Handle(PaymentServiceCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            var lightningFees = _config["ServiceCharge:LightningFeeSats"];
            var nairaFees = _config["ServiceCharge:NairaFee"];
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user == null)
                {
                    return Result.Failure("Unable to make payment. Invalid user details");
                }
                switch (request.PaymentModeType)
                {
                    case PaymentModeType.Bitcoin:
                        return Result.Failure("Coming soon. Kindly choose other form of payment");
                    case PaymentModeType.Lightning:
                        var generateLightning = await _lightningService.CreateInvoice(long.Parse(lightningFees), request.Email, UserType.Admin);
                        if (string.IsNullOrEmpty(generateLightning))
                        {
                            return Result.Failure("An error occured while generating invoice");
                        }
                        return Result.Success("Invoice generation was successful", generateLightning);
                        break;
                    case PaymentModeType.Fiat:
                        var paystackRequest = new PaystackPaymentRequest
                        {
                            Name = user.user.Name,
                            Reference = reference,
                            Email = request.Email,
                            Amount = int.Parse(nairaFees)
                        };
                        var paystackInitialtion = await _paystackService.MakePayment(paystackRequest);
                        return Result.Success("Paystack initiation was successful", paystackInitialtion);
                        break;
                    default:
                        break;
                }
                return Result.Success("done");
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Service payment failed", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
