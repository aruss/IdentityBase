using System;
using System.Threading.Tasks;
using IdentityBase.Public.EntityFramework.Entities;
using IdentityBase.Public.EntityFramework.Extensions;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace IdentityBase.Public.EntityFramework
{
    public class MigrationDbContext :
        DbContext,
        IConfigurationDbContext, 
        IPersistedGrantDbContext, 
        IUserAccountDbContext
    {
        private readonly EntityFrameworkOptions storeOptions;

        public MigrationDbContext(
            DbContextOptions<MigrationDbContext> options,
            EntityFrameworkOptions storeOptions)
            : base(options)
        {
            this.storeOptions = storeOptions ??
                throw new ArgumentNullException(nameof(storeOptions));
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<ExternalAccount> ExternalAccounts { get; set; }
        public DbSet<UserAccountClaim> UserAccountClaims { get; set; }
        
        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureClientContext(storeOptions);
            modelBuilder.ConfigureResourcesContext(storeOptions);
            modelBuilder.PersistedGrantDbContext(storeOptions);
            modelBuilder.UserAccountDbContext(storeOptions);

            base.OnModelCreating(modelBuilder);
        }
    }
}
