using Microsoft.EntityFrameworkCore;
using ServiceBase.IdentityServer.Public.EntityFramework.Entities;
using System;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EntityFramework.Interfaces
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