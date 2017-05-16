using Microsoft.EntityFrameworkCore;
using IdentityBase.Public.EntityFramework.Entities;
using IdentityBase.Public.EntityFramework.Extensions;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Options;
using System;
using System.Threading.Tasks;

namespace IdentityBase.Public.EntityFramework.DbContexts
{
    public class UserAccountDbContext : DbContext, IUserAccountDbContext
    {
        private readonly EntityFrameworkOptions _options;

        public UserAccountDbContext(DbContextOptions<UserAccountDbContext> dbContextOptions, EntityFrameworkOptions options)
            : base(dbContextOptions)
        {
            if (options == null) throw new ArgumentNullException(nameof(_options));
            _options = options;
        }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<ExternalAccount> ExternalAccounts { get; set; }
        public DbSet<UserAccountClaim> UserAccountClaims { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UserAccountDbContext(_options);

            base.OnModelCreating(modelBuilder);
        }
    }
}