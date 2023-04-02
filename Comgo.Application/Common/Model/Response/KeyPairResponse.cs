using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response
{
    public class KeyPairResponse
    {
        public string Mnemonic { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
