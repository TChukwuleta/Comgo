using Comgo.Application.Common.Model.Response;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;
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
        Task<(bool success, string message)> CreateUserWallet(User user);
        Task<(bool success, string message)> GenerateAddress(string userId);
        Task<(bool success, string message)> GenerateAddressAsync(string userId);
        Task<(bool success, string message)> ListTransactions(string userid);
        Task<(bool success, string message)> SendToAddress(string address, string amountBtc);
        Task<(bool success, string message)> ImportDescriptor(string adminDescriptor, User user);
        Task<(bool success, string message)> GenerateDescriptor(string walletname, User user);
        Task<(bool success, string message)> CreateDescriptorString(string pubkeyone, string pubkeytwo);
        Task<(bool success, WalletBalance response)> GetWalletBalance(string userId);
        Task<(bool success, string message, KeyPairResponse entity)> CreateNewKeyPairAsync(string userId, string publicKey);
        Task<(bool success, string message)> ConfirmUserTransaction(string userId, string reference);
        Task<(bool success, string message)> CreatePSBTAsync(string userId, string destinationAddress, decimal amount);
        Task<(bool success, string message, decimal amount)> GetDescriptorBalance(string userId);
    }
}
