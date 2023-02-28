using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class Unit : GeneralEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Key { get; set; }
        public string KeyPhrase { get; set; }
        public string UserId { get; set; }
        public string ParentUserId { get; set; }
    }
}
