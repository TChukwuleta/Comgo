using Comgo.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class MultisigAddressCreationResponse
    {
        public MultisigResult result { get; set; }
        public object error { get; set; }
        public string id { get; set; }
    }

    public class MultisigResult
    {
        public string address { get; set; }
        public string redeemScript { get; set; }
        public string descriptor { get; set; }
    }
}
