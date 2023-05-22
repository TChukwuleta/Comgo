using Comgo.Application.Common.Model.Request;
using Comgo.Application.Common.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IPaystackService
    {
        Task<(bool success, PaystackInitializationResponse response, string message)> MakePayment(PaystackPaymentRequest request);
        Task<bool> VerifyPayment(string reference);
    }
}