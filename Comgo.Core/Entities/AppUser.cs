﻿using Comgo.Core.Enums;
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
        public string Descriptor { get; set; }
        public bool IsWalletCreated { get; set; }
        public string PublicKey { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public string WalletName { get; set; }
        public string UserId { get; set; }
        public Status Status { get; set; }
        public UserType UserType { get; set; }
        public string StatusDesc { get { return Status.ToString(); } }
        public int? UserCount { get; set; }
    }
}
