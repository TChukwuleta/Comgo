using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IEncryptionService
    {
        string EncryptData(string request);
        string DecryptData(string request);
    }
}
