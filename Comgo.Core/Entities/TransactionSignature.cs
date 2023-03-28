using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class TransactionSignature : GeneralEntity
    {
        public bool UserSigned { get; set; }
        public bool AdminSigned { get; set; }
        public string TxnHex { get; set; }
        public string UserId { get; set; }
    }
}
