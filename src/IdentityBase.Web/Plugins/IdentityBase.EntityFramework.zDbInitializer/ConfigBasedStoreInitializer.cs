namespace IdentityBase.EntityFramework.DbInitializer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using IdentityBase.Configuration;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using ServiceBase.Extensions;

    public class ConfigBasedStoreInitializer : IExampleDataStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<ConfigBasedStoreInitializer> _logger;
        private readonly MigrationDbContext _migrationDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;
        private readonly IHostingEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;

        public ConfigBasedStoreInitializer(
            EntityFrameworkOptions options,
            ApplicationOptions appOptions,
            ILogger<ConfigBasedStoreInitializer> logger,
            MigrationDbContext migrationDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext,
            IHostingEnvironment environment,
            IServiceProvider serviceProvider)
        {
            this._options = options;
            this._appOptions = appOptions;
            this._logger = logger;
            this._migrationDbContext = migrationDbContext;
            this._configurationDbContext = configurationDbContext;
            this._persistedGrantDbContext = persistedGrantDbContext;
            this._userAccountDbContext = userAccountDbContext;
            this._environment = environment;

            this._serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this._logger.LogDebug("ConfigBasedStoreInitializer initialized");
        }

        /// <summary>
        /// Creates a database tables and seeds example data if needed
        /// </summary>
        public void InitializeStores()
        {
            this._logger.LogDebug("Initialize Stores, MigrateDatabase: " +
                $"{_options.MigrateDatabase}, SeedExampleData: " +
                $"{_options.SeedExampleData}");

            if (_options.MigrateDatabase)
            {
                this._logger.LogDebug("Try migrate database");
                this._migrationDbContext.Database.Migrate();
                this._logger.LogDebug("Database migrated");
            }

            if (this._options.SeedExampleData)
            {
                this._logger.LogDebug("Try seed initial data");
                this.EnsureSeedData();
                this._logger.LogDebug("Initial data seeded");
            }

            // if (this._options.EnableTokenCleanup)
            // {
            //     using (var serviceScope = _serviceProvider
            //         .GetRequiredService<IServiceScopeFactory>()
            //         .CreateScope())
            //     {
            //         serviceScope.ServiceProvider
            //             .GetService<TokenCleanupService>().Start();
            //     }
            // }
        }

        public void CleanupStores()
        {
            this._logger.LogDebug(
                $"Cleanup Stores, EnsureDeleted: {_options.EnsureDeleted}");

            if (this._options.EnsureDeleted)
            {
                this._logger.LogDebug("Ensure deleting database");
                this._migrationDbContext.Database.EnsureDeleted();
                this._logger.LogDebug("Database deleted");
            }

            // if (this._options.EnableTokenCleanup)
            // {
            //     using (var serviceScope = _serviceProvider
            //         .GetRequiredService<IServiceScopeFactory>()
            //         .CreateScope())
            //     {
            //         serviceScope.ServiceProvider
            //             .GetService<TokenCleanupService>().Stop();
            //     }
            // }
        }

        internal virtual void EnsureSeedData()
        {
            this._logger.LogDebug("Ensure Seed Data");

            string rootPath = _options.SeedExampleDataPath.GetFullPath(
                this._environment.ContentRootPath);

            if (!_configurationDbContext.IdentityResources.Any())
            {
                string path = Path.Combine(rootPath,
                    "data_resources_identity.json");

                this._logger.LogDebug($"Loading file: {path}");

                var resources = JsonConvert
                    .DeserializeObject<List<IdentityResource>>(
                        File.ReadAllText(path));

                foreach (var resource in resources)
                {
                    this._configurationDbContext.IdentityResources
                        .Add(resource.ToEntity());
                }

                this._configurationDbContext.SaveChanges();
                this._logger.LogDebug("Saved Resource Identities");
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                string path = Path.Combine(rootPath, "data_resources_api.json");
                this._logger.LogDebug($"Loading file: {path}");

                var resources = JsonConvert
                    .DeserializeObject<List<ApiResource>>(
                        File.ReadAllText(path));

                foreach (var resource in resources)
                {
                    this._configurationDbContext.ApiResources
                        .Add(resource.ToEntity());
                }

                this._configurationDbContext.SaveChanges();
                this._logger.LogDebug("Saved Resource API");
            }

            if (!this._configurationDbContext.Clients.Any())
            {
                string path = Path.Combine(rootPath, "data_clients.json");
                this._logger.LogDebug($"Loading file: {path}");

                List<Client> clients = JsonConvert
                    .DeserializeObject<List<Client>>(File.ReadAllText(path));

                foreach (var client in clients)
                {
                    Entities.Client entity = client.ToEntity();
                    entity.Id = Guid.NewGuid();

                    this._configurationDbContext.Clients.Add(entity);
                }
                this._configurationDbContext.SaveChanges();
                this._logger.LogDebug("Saved Clients");
            }

            if (!this._userAccountDbContext.UserAccounts.Any())
            {
                string path = Path.Combine(rootPath, "data_users.json");
                this._logger.LogDebug($"Loading file: {path}");

                List<UserAccount> userAccounts = JsonConvert
                    .DeserializeObject<List<UserAccount>>(
                        File.ReadAllText(path));

                foreach (var userAccount in userAccounts)
                {
                    this._userAccountDbContext.UserAccounts
                        .Add(userAccount.ToEntity());
                }

                this._userAccountDbContext.SaveChanges();
                this._logger.LogDebug("Saved Users");
            }
        }
    }
}