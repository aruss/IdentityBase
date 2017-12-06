namespace IdentityBase.EntityFramework.DbContexts
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.Entities;
    using IdentityBase.EntityFramework.Extensions;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.EntityFrameworkCore;

    public class UserAccountDbContext : DbContext, IUserAccountDbContext
    {
        private readonly EntityFrameworkOptions _options;

        public UserAccountDbContext(
            DbContextOptions<UserAccountDbContext> dbContextOptions,
            EntityFrameworkOptions options)
            : base(dbContextOptions)
        {
            this._options = options ??
                throw new ArgumentNullException(nameof(this._options));
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
            modelBuilder.ConfigureUserAccountContext(this._options);

            base.OnModelCreating(modelBuilder);
        }
    }
}