using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IBitcoinService
    {
        Task<string> GenerateAddress(string privkey, string userId);
        Task<MultisigAddressCreationResponse> GenerateMultisigAddressBitcoinCore(string userpublickey, string adminpublickey, string methodname, int minimumKeys);
        Task<Signature> GenerateSystemKey(string userId);
        Task<string> GetRawTransactionBitcoinCore(string txnid);
        Task<string> ConfirmUserTransaction(string privKey, string userId, string email);
        Task<string> TestMultisig(string privKey, string userId, string email);
    }
}
