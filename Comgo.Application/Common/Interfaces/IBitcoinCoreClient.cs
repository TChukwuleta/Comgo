using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IBitcoinCoreClient
    {
        Task<string> BitcoinRequestServer(string methodName, List<JToken> parameters);
        Task<string> BitcoinRequestServer(string methodName, List<string> parameters);
        Task<string> BitcoinRequestServer(string methodName, List<JToken> parameters, int count);
        Task<string> BitcoinRequestServer(string methodName, List<string> parameters, int count);
        Task<string> BitcoinRequestServer(string methodName, string parameters);
        Task<string> BitcoinRequestServer(string methodName, string parameters, int value);
        Task<string> BitcoinRequestServer(string methodName);
        Task<string> WalletInformation(string walletname, string methodname);
    }
}
