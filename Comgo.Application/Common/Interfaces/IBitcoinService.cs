using Comgo.Core.Entities;

namespace Comgo.Application.Common.Interfaces
{
    public interface IBitcoinService
    {
        Task<(bool success, string message)> CreateUserWallet(User user);
        Task<(bool success, string message)> ValidateTransactionFromSender(string userId, string reference, string destinationAddress);
        Task<(bool success, string message)> ImportDescriptor(User user);
        Task<(bool success, string message)> GenerateDescriptor(User user);
        Task<(bool success, string message)> BroadcastTransaction(string walletname, string hex);
        Task<(bool success, string message)> GenerateDescriptorAddress(string descriptor);
        Task<(bool success, string message)> CombinePSBTAsync(string walletname, string psbt_one, string psbt_two);
        Task<(bool success, string message, bool isComplete)> ProcessPSBTAsync(string walletname, string psbt);
        Task<(bool success, string message)> FinalizePSBTAsync(string walletname, string psbt);
        Task<(bool success, Model.Response.BitcoinCommandResponses.PSBTResponse message)> CreateWalletPSTAsync(decimal amount, string address);
        Task<(bool success, string message, decimal amount)> GetDescriptorBalance(string descriptor);
    }
}
