using Comgo.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasPaid { get; set; }
        public decimal Balance { get; set; }
        public string UserId { get; set; }
        public Status Status { get; set; }
        public ICollection<Unit> Units { get; set; }
        public string StatusDesc { get { return Status.ToString(); } }
    }
}
