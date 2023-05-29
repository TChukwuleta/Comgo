using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model.Response
{
    public class PSBTResponse
    {
        public string InitialPSBT { get; set; }
        public string SystemSignedPSBT { get; set; }
        public string UserSignedPSBT { get; set; }
        public string FinalizedPSBT { get; set; }
    }
}
