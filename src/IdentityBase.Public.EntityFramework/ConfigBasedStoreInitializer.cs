using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Public.EntityFramework.Options;
using IdentityBase.Services;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdentityBase.Public.EntityFramework
{
    public class ConfigBasedStoreInitializer : IStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<ConfigBasedStoreInitializer> _logger;
        private readonly MigrationDbContext _migrationDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        private readonly IHostingEnvironment _environment;

        public ConfigBasedStoreInitializer(
            EntityFrameworkOptions options,
            ApplicationOptions appOptions,
            ILogger<ConfigBasedStoreInitializer> logger,
            MigrationDbContext migrationDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext,
            IHostingEnvironment environment)
        {
            _options = options;
            _appOptions = appOptions;
            _logger = logger;
            _migrationDbContext = migrationDbContext;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _userAccountDbContext = userAccountDbContext;
            _environment = environment;

            _logger.LogDebug("ConfigBasedStoreInitializer initialized");
        }

        public void InitializeStores()
        {
            _logger
                .LogDebug($"Initialize Stores, MigrateDatabase: " +
                $"{_options.MigrateDatabase}, SeedExampleData: {_options.SeedExampleData}");

            // Only a leader may migrate or seed 
            if (_appOptions.Leader)
            {
                if (_options.MigrateDatabase)
                {
                    _logger.LogDebug("Try migrate database");
                    _migrationDbContext.Database.Migrate();
                    _logger.LogDebug("Database migrated");
                }

                if (_options.SeedExampleData)
                {
                    _logger.LogDebug("Try seed initial data");
                    this.EnsureSeedData();
                    _logger.LogDebug("Initial data seeded");
                }
            }
        }

        public void CleanupStores()
        {
            _logger
                .LogDebug($"Cleanup Stores, Leader: {_appOptions.Leader}, " +
                $"EnsureDeleted: {_options.EnsureDeleted}");

            // Only leader may delete the database 
            if (_appOptions.Leader && _options.EnsureDeleted)
            {
                _logger.LogDebug("Ensure deleting database");
                _migrationDbContext.Database.EnsureDeleted();
                _logger.LogDebug("Database deleted");
            }
        }

        internal virtual void EnsureSeedData()
        {
            _logger.LogDebug("Ensure Seed Data");

            var rootPath = _options.SeedExampleDataPath.GetFullPath(_environment.ContentRootPath);

            if (!_configurationDbContext.IdentityResources.Any())
            {
                var path = Path.Combine(rootPath, "data_resources_identity.json");

                _logger.LogDebug($"Loading file: {path}");

                var resources = JsonConvert
                    .DeserializeObject<List<IdentityResource>>(File.ReadAllText(path));

                foreach (var resource in resources)
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }

                _configurationDbContext.SaveChanges();
                _logger.LogDebug("Saved Resource Identities");
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                var path = Path.Combine(rootPath, "data_resources_api.json");
                _logger.LogDebug($"Loading file: {path}");

                var resources = JsonConvert
                    .DeserializeObject<List<ApiResource>>(File.ReadAllText(path));

                foreach (var resource in resources)
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }

                _configurationDbContext.SaveChanges();
                _logger.LogDebug("Saved Resource API");
            }

            if (!_configurationDbContext.Clients.Any())
            {
                var path = Path.Combine(rootPath, "data_clients.json");
                _logger.LogDebug($"Loading file: {path}");

                var clients = JsonConvert
                    .DeserializeObject<List<Client>>(File.ReadAllText(path));

                foreach (var client in clients)
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
                _logger.LogDebug("Saved Clients");
            }

            if (!_userAccountDbContext.UserAccounts.Any())
            {
                var path = Path.Combine(rootPath, "data_users.json");
                _logger.LogDebug($"Loading file: {path}");

                var userAccounts = JsonConvert
                    .DeserializeObject<List<UserAccount>>(File.ReadAllText(path));

                foreach (var userAccount in userAccounts)
                {
                    _userAccountDbContext.UserAccounts.Add(userAccount.ToEntity());
                }

                _userAccountDbContext.SaveChanges();
                _logger.LogDebug("Saved Users");
            }
        }
    }
}