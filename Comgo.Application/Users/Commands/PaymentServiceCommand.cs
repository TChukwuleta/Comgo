using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators;
using Comgo.Application.Common.Model.Request;
using Comgo.Application.Common.Model.Response;
using Comgo.Core.Enums;
using Comgo.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Drawing;

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
        private readonly ICloudinaryService _cloudinaryService;
        public PaymentServiceCommandHandler(IConfiguration config, IAuthService authService, IPaystackService paystackService, 
            ILightningService lightningService, ICloudinaryService cloudinaryService)
        {
            _config = config;
            _lightningService = lightningService;
            _authService = authService;
            _paystackService = paystackService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result> Handle(PaymentServiceCommand request, CancellationToken cancellationToken)
        {
            var reference = $"Comgo_{DateTime.Now.Ticks}";
            var lightningFees = int.Parse(_config["ServiceCharge:LightningFeeSats"]);
            var nairaFees = _config["ServiceCharge:NairaFee"];
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user == null)
                {
                    return Result.Failure("Unable to make payment. Invalid user details");
                }

                if (!user.user.EmailConfirmed)
                {
                    return Result.Failure("Kindly confirm verify your account before you proceed to make payment");
                }
                switch (request.PaymentModeType)
                {
                    case PaymentModeType.Bitcoin:
                        return Result.Success("Coming soon. Kindly choose other form of payment");
                    case PaymentModeType.Lightning:
                        var generateLightning = await _lightningService.CreateInvoice(lightningFees, request.Email);
                        if (string.IsNullOrEmpty(generateLightning))
                        {
                            return Result.Failure("An error occured while generating invoice");
                        }
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(generateLightning, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(10);
                        Random random = new Random();
                        var location = $"{random.Next()}";
                        qrCodeImage.Save($"{location}.jpg");
                        var invoice = await _cloudinaryService.UploadInvoiceQRCode(location);
                        PaystackInitializationResponse lightningResponse = new()
                        {
                            authorization_url = generateLightning,
                            reference = reference,
                            image = invoice
                        };
                        return Result.Success("Invoice generation was successful", lightningResponse);
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
                        if (!paystackInitialtion.success)
                        {
                            return Result.Failure(paystackInitialtion.message);
                        }
                        return Result.Success("Paystack initiation was successful", paystackInitialtion.response);
                    default:
                        return Result.Failure("Invalid payment mode type selected");
                }
                return Result.Success("done");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Service payment failed. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
