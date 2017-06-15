using IdentityBase.Public.EntityFramework.Entities;
using IdentityBase.Public.EntityFramework.Extensions;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IdentityBase.Public.EntityFramework
{
    public class MigrationDbContext : DbContext, IConfigurationDbContext, IPersistedGrantDbContext, IUserAccountDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<ExternalAccount> ExternalAccounts { get; set; }
        public DbSet<UserAccountClaim> UserAccountClaims { get; set; }

        public MigrationDbContext()
        {

        }

        public MigrationDbContext(DbContextOptions<MigrationDbContext> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            // make sure the connection string makes sense for your machine
            optionsBuilder.UseSqlServer(@"Server=(local);Database=EfCoreSample;Trusted_Connection=True;");
#endif
        }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var options = new EntityFrameworkOptions();
            modelBuilder.ConfigureClientContext(options);
            modelBuilder.ConfigureResourcesContext(options);
            modelBuilder.PersistedGrantDbContext(options);
            modelBuilder.UserAccountDbContext(options);

            base.OnModelCreating(modelBuilder);
        }
    }
}
