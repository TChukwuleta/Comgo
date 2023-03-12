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
        Task<(bool success, string message)> GenerateAddress(string userId);
        Task<(bool success, string message)> CreateNewKeyPair(string userId);
        Task<(bool success, string message)> ConfirmUserTransaction(string userId, string email);
        Task<string> CreateMultisigTransaction(string userid, string recipient);
    }
}
