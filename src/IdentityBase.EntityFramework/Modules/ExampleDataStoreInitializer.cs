namespace IdentityBase.EntityFramework
{
    using System.Linq;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Mappers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class ExampleDataStoreInitializer
    {
        private readonly EntityFrameworkOptions options;
        private readonly ApplicationOptions appOptions;
        private readonly ILogger<ConfigBasedStoreInitializer> logger;
        private readonly MigrationDbContext migrationDbContext;
        private readonly IConfigurationDbContext configurationDbContext;
        private readonly IPersistedGrantDbContext persistedGrantDbContext;
        private readonly IUserAccountDbContext userAccountDbContext;
        private readonly ICrypto crypto;

        public ExampleDataStoreInitializer(
            EntityFrameworkOptions options,
            ApplicationOptions appOptions,
            ILogger<ConfigBasedStoreInitializer> logger,
            MigrationDbContext migrationDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext,
            ICrypto crypto)
        {
            this.options = options;
            this.appOptions = appOptions;
            this.logger = logger;
            this.migrationDbContext = migrationDbContext;
            this.configurationDbContext = configurationDbContext;
            this.persistedGrantDbContext = persistedGrantDbContext;
            this.userAccountDbContext = userAccountDbContext;
            this.crypto = crypto;
        }

        public void InitializeStores()
        {
            if (this.options.MigrateDatabase)
            {
                this.logger.LogInformation("Try migrate database");
                this.migrationDbContext.Database.Migrate(); 
            }

            if (this.options.SeedExampleData)
            {
                this.logger.LogInformation("Try seed initial data");
                this.EnsureSeedData();
            }            
        }

        public void CleanupStores()
        {
            if (this.options.EnsureDeleted)
            {
                this.logger.LogInformation("Ensure deleting database");
                this.migrationDbContext.Database.EnsureDeleted();
            }
        }

        internal virtual void EnsureSeedData()
        {
            var exampleData = new ExampleData();

            if (!this.configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in exampleData.GetIdentityResources())
                {
                    this.configurationDbContext.IdentityResources
                        .Add(resource.ToEntity());
                }
                this.configurationDbContext.SaveChanges();
            }

            if (!this.configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in exampleData.GetApiResources())
                {
                    this.configurationDbContext.ApiResources
                        .Add(resource.ToEntity());
                }
                this.configurationDbContext.SaveChanges();
            }

            if (!this.configurationDbContext.Clients.Any())
            {
                foreach (var client in exampleData.GetClients())
                {
                    this.configurationDbContext.Clients.Add(client.ToEntity());
                }
                this.configurationDbContext.SaveChanges();
            }

            if (!this.userAccountDbContext.UserAccounts.Any())
            {
                foreach (var userAccount in exampleData
                    .GetUserAccounts(this.crypto, this.appOptions))
                {
                    this.userAccountDbContext.UserAccounts
                        .Add(userAccount.ToEntity());
                }
                this.userAccountDbContext.SaveChanges();
            }
        }
    }
}