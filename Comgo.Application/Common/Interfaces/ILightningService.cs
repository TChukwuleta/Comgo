using Comgo.Application.Common.Model.Response;
using Comgo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface ILightningService
    {
        Task<string> CreateInvoice(long satoshis, string message, UserType userType);
        Task<long> GetChannelBalance(UserType userType);
        Task<long> GetWalletBalance(UserType userType);
        Task<string> SendLightning(string paymentRequest, UserType userType);
        Task<InvoiceSettlementResponse> ListenForSettledInvoice(UserType userType);
    }
}
