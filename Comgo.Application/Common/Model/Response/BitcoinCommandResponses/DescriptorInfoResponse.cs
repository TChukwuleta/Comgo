using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class DescriptorInfoResponse
    {
        public string descriptor { get; set; }
        public string checksum { get; set; }
        public bool isrange { get; set; }
        public bool issolvable { get; set; }
        public bool hasprivatekeys { get; set; }
    }
}
