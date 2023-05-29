using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class PSBT : GeneralEntity
    {
        public string InitialPSBT { get; set; }
        public string UserSignedPSBT { get; set; }
        public string SystemSignedPSBT { get; set; }
        public string FinalizedPSBT { get; set; }
        public string Reference { get; set; }
        public bool ShouldProcessPSBT { get; set; }
        public string UserId { get; set; }
    }
}
