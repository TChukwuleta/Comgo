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
        Task<Signature> GenerateSystemKey(string userId);
        Task<string> ConfirmUserTransaction(string privKey, string userId, string email);
    }
}
