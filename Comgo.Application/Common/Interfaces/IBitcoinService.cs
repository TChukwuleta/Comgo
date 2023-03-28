using Comgo.Application.Common.Model.Response;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IBitcoinService
    {
        Task<(bool success, string message)> GenerateAddress(string userId);
        Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPair(string userId);
        Task<Transaction> SignSignature(BitcoinAddress recipientAddress, Money amount, ScriptCoin coin, Money minerFee, BitcoinAddress changeAddress, PubKey userKey, PubKey systemKey);
        Task<(bool success, string message)> ConfirmUserTransaction(string userId, string email);
        Task<(bool success, string message)> CreateMultisigTransaction(string userid, string recipient);
    }
}
