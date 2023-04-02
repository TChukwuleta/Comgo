using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response
{
    public class WalletBalance
    {
        public string Immature { get; set; }
        public string Available { get; set; }
        public string Unconfirmed { get; set; }
        public string Confirmed { get; set; }
        public string Total { get; set; }
    }
}
