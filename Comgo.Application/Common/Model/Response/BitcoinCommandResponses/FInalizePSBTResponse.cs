using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class FInalizePSBTResponse
    {
        public string psbt { get; set; }
        public string hex { get; set; }
        public bool complete { get; set; }
    }
}
