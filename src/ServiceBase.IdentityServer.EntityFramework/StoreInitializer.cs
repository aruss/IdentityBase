using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceBase.IdentityServer.Configuration;
using ServiceBase.IdentityServer.Crypto;
using ServiceBase.IdentityServer.EntityFramework.DbContexts;
using ServiceBase.IdentityServer.EntityFramework.Interfaces;
using ServiceBase.IdentityServer.EntityFramework.Mappers;
using ServiceBase.IdentityServer.EntityFramework.Options;
using ServiceBase.IdentityServer.Services;
using System.Linq;

namespace ServiceBase.IdentityServer.EntityFramework
{
    public class DefaultStoreInitializer : IStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ILogger<DefaultStoreInitializer> _logger;
        private readonly IHostingEnvironment _env;
        private readonly DefaultDbContext _defaultDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        private readonly ICrypto _crypto;
        private readonly ApplicationOptions _applicationOptions;

        public DefaultStoreInitializer(
            IOptions<EntityFrameworkOptions> options,
            ILogger<DefaultStoreInitializer> logger,
            IHostingEnvironment env,
            DefaultDbContext defaultDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext,
            ICrypto crypto,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _options = options.Value;
            _logger = logger;
            _env = env;
            _defaultDbContext = defaultDbContext;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _userAccountDbContext = userAccountDbContext;
            _crypto = crypto;
            _applicationOptions = applicationOptions.Value;

        }

        public void InitializeStores()
        {
            if (_options.MigrateDatabase)
            {
                this.MigrateDatabase();
            }

            if (_options.SeedExampleData)
            {
                this.EnsureSeedData();
            }
        }

        internal virtual void MigrateDatabase()
        {
            // _defaultDbContext.Database.EnsureCreated();
            _defaultDbContext.Database.Migrate();
        }

        internal virtual void EnsureSeedData()
        {
            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in Clients.Get().ToList())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in Resources.GetIdentityResources().ToList())
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in Resources.GetApiResources().ToList())
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_userAccountDbContext.UserAccounts.Any())
            {
                foreach (var userAccount in UserAccounts.Get(_crypto, _applicationOptions).ToList())
                {
                    _userAccountDbContext.UserAccounts.Add(userAccount.ToEntity());
                }
                _userAccountDbContext.SaveChanges();
            }
        }
    }
}