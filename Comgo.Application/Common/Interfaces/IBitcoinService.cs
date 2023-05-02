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
        Task<(bool success, string message)> GenerateAddressAsync(string userId);
        Task<(bool success, string message)> TransactionLookOut();
        Task<(bool success, string message)> ListTransactions(string userid);
        Task<(bool success, string message)> SendToAddress(string address, string amountBtc);
        Task<(bool success, WalletBalance response)> GetWalletBalance(string userId);
        Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPair(string userId, string password);
        Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPairAsync(string userId);
        Task<Transaction> SignSignature(BitcoinAddress recipientAddress, Money amount, ScriptCoin coin, Money minerFee, BitcoinAddress changeAddress, PubKey userKey, PubKey systemKey);
        Task<(bool success, string message)> ConfirmUserTransaction(string userId, string reference);
        Task<(bool success, string message)> CreateMultisigTransaction(string userid, string recipient);
        Task<(bool success, string message)> CreatePSBT(string userid, string recipient);
    }
}
