using Comgo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<AppUser> AppUsers { get; set; }
        DbSet<UserCustody> UserCustodies { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Signature> Signatures { get; set; }
        DbSet<Unit> Units { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
