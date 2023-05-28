using Comgo.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Core.Entities
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }
        public bool IsWalletCreated { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public string Descriptor { get; set; }
        public string Walletname { get; set; }
        public string PublicKey { get; set; }
        public Status Status { get; set; }
        public UserType UserType { get; set; }
        public bool EmailConfirmed { get; set; }
        public int? UserCount { get; set; }
    }
}
