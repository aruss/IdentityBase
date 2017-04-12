using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceBase.IdentityServer.Models;
using ServiceBase.IdentityServer.Public.EntityFramework.Interfaces;
using ServiceBase.IdentityServer.Public.EntityFramework.Mappers;
using ServiceBase.IdentityServer.Public.EntityFramework.Options;
using ServiceBase.IdentityServer.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServiceBase.IdentityServer.Public.EntityFramework
{
    public class DefaultStoreInitializer : IStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ILogger<DefaultStoreInitializer> _logger;
        private readonly DefaultDbContext _defaultDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        
        public DefaultStoreInitializer(
            EntityFrameworkOptions options,
            ILogger<DefaultStoreInitializer> logger,
            DefaultDbContext defaultDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext)
        {
            _options = options;
            _logger = logger;
            _defaultDbContext = defaultDbContext;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _userAccountDbContext = userAccountDbContext;
        }

        public void InitializeStores()
        {
            if (_options.MigrateDatabase)
            {
                _defaultDbContext.Database.Migrate();
            }

            if (_options.SeedExampleData)
            {
                this.EnsureSeedData();
            }
        }        

        internal virtual void EnsureSeedData()
        {
         
            if (!_configurationDbContext.IdentityResources.Any())
            {
                var resources = JsonConvert.DeserializeObject<List<IdentityResource>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_resources_identity.json")));
                foreach (var resource in resources)
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                var resources = JsonConvert.DeserializeObject<List<ApiResource>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_resources_api.json")));
                foreach (var resource in resources)
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.Clients.Any())
            {
                var clients = JsonConvert.DeserializeObject<List<Client>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_clients.json")));
                foreach (var client in clients)
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }
            
            if (!_userAccountDbContext.UserAccounts.Any())
            {
                var userAccounts = JsonConvert.DeserializeObject<List<UserAccount>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_users.json")));
                foreach (var userAccount in userAccounts)
                {
                    _userAccountDbContext.UserAccounts.Add(userAccount.ToEntity());
                }
                _userAccountDbContext.SaveChanges();
            }
        }
    }
}