using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Model.Request;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayStack.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<string> MakePayment(PaystackPaymentRequest request)
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
                    return JsonConvert.SerializeObject(response.Data);
                }
                return response.Message;
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
