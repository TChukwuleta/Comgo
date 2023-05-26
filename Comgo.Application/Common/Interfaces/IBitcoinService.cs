using Comgo.Application.Common.Model.Response;
using Comgo.Application.Common.Model.Response.BitcoinCommandResponses;
using Comgo.Core.Entities;

namespace Comgo.Application.Common.Interfaces
{
    public interface IBitcoinService
    {
        Task<(bool success, string message)> CreateUserWallet(User user);
        Task<(bool success, string message)> ValidateTransactionFromSender(string userId, string reference, string destinationAddress);
        Task<(bool success, string message)> ImportDescriptor(User user);
        Task<(bool success, string message)> GenerateDescriptor(User user);
        Task<(bool success, string message)> GenerateDescriptorAddress(string descriptor);
        Task<(bool success, object message)> ProcessPSBTAsync(string walletname, string psbt);
        Task<(bool success, object message)> FinalizePSBTAsync(string walletname, string psbt);
        Task<(bool success, PSBTResponse message)> CreateWalletPSTAsync(decimal amount, string address);
        Task<(bool success, string message, decimal amount)> GetDescriptorBalance(string descriptor);
    }
}
