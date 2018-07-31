namespace IdentityBase.EntityFramework.DbInitializer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Mappers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class ExampleDataStoreInitializer : IExampleDataStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<ConfigBasedStoreInitializer> _logger;
        private readonly MigrationDbContext _migrationDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        private readonly ICryptoService _cryptoService;

        public ExampleDataStoreInitializer(
            EntityFrameworkOptions options,
            ApplicationOptions appOptions,
            ILogger<ConfigBasedStoreInitializer> logger,
            MigrationDbContext migrationDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext,
            ICryptoService cryptoService)
        {
            this._options = options;
            this._appOptions = appOptions;
            this._logger = logger;
            this._migrationDbContext = migrationDbContext;
            this._configurationDbContext = configurationDbContext;
            this._persistedGrantDbContext = persistedGrantDbContext;
            this._userAccountDbContext = userAccountDbContext;
            this._cryptoService = cryptoService;
        }

        public void InitializeStores()
        {
            if (this._options.MigrateDatabase)
            {
                this._logger.LogInformation("Try migrate database");
                this._migrationDbContext.Database.Migrate();
            }

            if (this._options.SeedExampleData)
            {
                this._logger.LogInformation("Try seed initial data");
                this.EnsureSeedData();
            }
        }

        public void CleanupStores()
        {
            if (this._options.EnsureDeleted)
            {
                this._logger.LogInformation("Ensure deleting database");
                this._migrationDbContext.Database.EnsureDeleted();
            }
        }

        internal virtual void EnsureSeedData()
        {
            var exampleData = new ExampleData();


            if (!this._configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in exampleData.GetIdentityResources())
                {
                    this._configurationDbContext.IdentityResources
                        .Add(resource.ToEntity());
                }
                this._configurationDbContext.SaveChanges();
            }

            if (!this._configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in exampleData.GetApiResources())
                {
                    this._configurationDbContext.ApiResources
                        .Add(resource.ToEntity());
                }
                this._configurationDbContext.SaveChanges();
            }

            if (!this._configurationDbContext.Clients.Any())
            {
                var entities = new List<Entities.Client>();

                foreach (var client in exampleData.GetClients())
                {
                    var entity = client.ToEntity();
                    entity.Id = System.Guid.NewGuid();

                    entities.Add(entity);

                    this._configurationDbContext.Clients.Add(entity);
                }
                this._configurationDbContext.SaveChanges();
            }

            if (!this._userAccountDbContext.UserAccounts.Any())
            {
                foreach (var userAccount in exampleData
                    .GetUserAccounts(this._cryptoService, this._appOptions))
                {
                    this._userAccountDbContext.UserAccounts
                        .Add(userAccount.ToEntity());
                }
                this._userAccountDbContext.SaveChanges();
            }
        }
    }
}