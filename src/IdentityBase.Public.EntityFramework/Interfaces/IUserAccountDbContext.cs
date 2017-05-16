using Microsoft.EntityFrameworkCore;
using IdentityBase.Public.EntityFramework.Entities;
using System;
using System.Threading.Tasks;

namespace IdentityBase.Public.EntityFramework.Interfaces
{
    public interface IUserAccountDbContext : IDisposable
    {
        DbSet<UserAccount> UserAccounts { get; set; }
        DbSet<ExternalAccount> ExternalAccounts { get; set; }
        DbSet<UserAccountClaim> UserAccountClaims { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync();
    }
}