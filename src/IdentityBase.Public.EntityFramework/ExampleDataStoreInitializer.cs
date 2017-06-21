using System.Linq;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Public.EntityFramework.Options;
using IdentityBase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityBase.Public.EntityFramework
{
    public class ExampleDataStoreInitializer : IStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<ConfigBasedStoreInitializer> _logger;
        private readonly MigrationDbContext _migrationDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        private readonly ICrypto _crypto;

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
            _options = options;
            _appOptions = appOptions;
            _logger = logger;
            _migrationDbContext = migrationDbContext;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _userAccountDbContext = userAccountDbContext;
            _crypto = crypto;
        }

        public void InitializeStores()
        {
            // Only a leader may migrate or seed 
            if (_appOptions.Leader)
            {
                if (_options.MigrateDatabase)
                {
                    _logger.LogInformation("Try migrate database");
                    _migrationDbContext.Database.Migrate();
                }

                if (_options.SeedExampleData)
                {
                    _logger.LogInformation("Try seed initial data");
                    this.EnsureSeedData();
                }
            }
        }

        public void CleanupStores()
        {
            // Only leader may delete the database 
            if (_appOptions.Leader && _options.EnsureDeleted)
            {
                _logger.LogInformation("Ensure deleting database");
                _migrationDbContext.Database.EnsureDeleted();
            }
        }

        internal virtual void EnsureSeedData()
        {
            var exampleData = new ExampleData();

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in exampleData.GetIdentityResources())
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in exampleData.GetApiResources())
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in exampleData.GetClients())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_userAccountDbContext.UserAccounts.Any())
            {
                foreach (var userAccount in exampleData.GetUserAccounts(_crypto, _appOptions))
                {
                    _userAccountDbContext.UserAccounts.Add(userAccount.ToEntity());
                }
                _userAccountDbContext.SaveChanges();
            }
        }
    }
}