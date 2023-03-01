using Comgo.Application.Common.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IPaystackService
    {
        Task<string> MakePayment(PaystackPaymentRequest request);
        Task<bool> VerifyPayment(string reference);
    }
}
