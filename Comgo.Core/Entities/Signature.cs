using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class Signature : GeneralEntity
    {
        public string AdminUserId { get; set; }
        public string SystemKey { get; set; }
        public string UserKey { get; set; }
        public string UserId { get; set; }
    }
}
