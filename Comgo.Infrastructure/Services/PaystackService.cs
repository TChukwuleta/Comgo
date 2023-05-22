using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Request;
using Comgo.Application.Common.Model.Response;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayStack.Net;

namespace Comgo.Infrastructure.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly IConfiguration _config;
        private readonly PayStackApi payStack;
        private readonly string token;
        public PaystackService(IConfiguration config)
        {
            _config = config;
            token = _config["Paystack:SecretKey"];
            payStack = new PayStackApi(token);
        }

        public async Task<(bool success, PaystackInitializationResponse response, string message)> MakePayment(PaystackPaymentRequest request)
        {
            try
            {
                TransactionInitializeRequest paymentRequest = new()
                {
                    AmountInKobo = request.Amount * 100,
                    Email = request.Email,
                    Reference = request.Reference,
                    Currency = "NGN",
                    CallbackUrl = "http://localhost:7293/payment/verify"
                };

                TransactionInitializeResponse response = payStack.Transactions.Initialize(paymentRequest);
                if (response.Status)
                {
                    PaystackInitializationResponse paystackInitialization = new()
                    {
                        access_code = response.Data.AccessCode,
                        authorization_url = response.Data.AuthorizationUrl,
                        reference = response.Data.Reference
                    };
                    return (true, paystackInitialization, response.Message);
                }
                return (false, null, response.Message);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<bool> VerifyPayment(string reference)
        {
            try
            {
                TransactionVerifyResponse response = payStack.Transactions.Verify(reference);
                if (response.Data.Status == "success")
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
