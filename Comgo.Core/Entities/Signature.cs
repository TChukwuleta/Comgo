using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class Signature : GeneralEntity
    {
        public string UserPubKey { get; set; }
        public string? UserSafeDetails { get; set; }
        public string UserId { get; set; }
        public string? AdminPubKey { get; set; }
    }
}
