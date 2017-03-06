

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.IdentityServer.Public.EF.Entities;
using ServiceBase.IdentityServer.Public.EF.Extensions;
using ServiceBase.IdentityServer.Public.EF.Interfaces;
using ServiceBase.IdentityServer.Public.EF.Options;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EF
{
    // Only for `dotnet ef migrations` command
    // dotnet ef migrations add init --context DefaultDbContext
    public class Startup
    {
        static void Main()
        { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DefaultDbContext>((builder) =>
            {
                builder.UseSqlServer("Just for migration creation purposes");
            });
        }
    }

    public class DefaultDbContext : DbContext, IConfigurationDbContext, IPersistedGrantDbContext, IUserAccountDbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<ExternalAccount> ExternalAccounts { get; set; }
        public DbSet<UserAccountClaim> UserAccountClaims { get; set; }

        public DefaultDbContext()
        {

        }

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
            : base(options)
        {

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
