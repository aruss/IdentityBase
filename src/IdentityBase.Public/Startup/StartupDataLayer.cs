using Autofac;
using Autofac.Extensions.DependencyInjection;
using IdentityBase.Public.EntityFramework;
using IdentityBase.Services;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace IdentityBase.Public
{
    public static class StartupDataLayer
    {
        public static void ValidateDataLayerServices(this IContainer container, ILogger logger)
        {
            if (container.IsRegistered<IStoreInitializer>())
            {
                logger.LogInformation("IStoreInitializer registered.");
            }

            if (!container.IsRegistered<IClientStore>()) { throw new Exception("IClientStore not registered."); }
            if (!container.IsRegistered<IResourceStore>()) { throw new Exception("IResourceStore not registered."); }
            if (!container.IsRegistered<ICorsPolicyService>()) { throw new Exception("ICorsPolicyService not registered."); }
            if (!container.IsRegistered<IPersistedGrantStore>()) { throw new Exception("IPersistedGrantStore not registered."); }
            if (!container.IsRegistered<IUserAccountStore>()) { throw new Exception("IUserAccountStore not registered."); }
        }
    }


    public class EntityFrameworkInMemoryModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            var config = Program.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseInMemoryDatabase();
                };

                options.MigrateDatabase = config.GetSection("EntityFramework").GetValue<bool>("MigrateDatabase");
                options.SeedExampleData = config.GetSection("EntityFramework").GetValue<bool>("SeedExampleData");
                options.SeedExampleDataPath = Path.Combine(".", "Config");

                services.AddDefaultStoreInitializer(options);
            });

            builder.Populate(services);
        }
    }

    public class EntityFrameworkSqlServerModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            var config = Program.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                var migrationsAssembly = typeof(IServiceCollectionExtensions).GetTypeInfo().Assembly.GetName().Name;
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseSqlServer(config["EntityFramework:SqlServer:ConnectionString"], o => o.MigrationsAssembly(migrationsAssembly));
                };

                options.MigrateDatabase = config.GetSection("EntityFramework").GetValue<bool>("MigrateDatabase");
                options.SeedExampleData = config.GetSection("EntityFramework").GetValue<bool>("SeedExampleData");
                options.SeedExampleDataPath = Path.Combine(".", "Config");

                services.AddDefaultStoreInitializer(options);
            });

            builder.Populate(services);
        }
    }

    public class EntityFrameworkNpgsqlModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            var config = Program.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                var migrationsAssembly = typeof(IServiceCollectionExtensions).GetTypeInfo().Assembly.GetName().Name;
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseNpgsql(config["EntityFramework:Npgsql:ConnectionString"], o => o.MigrationsAssembly(migrationsAssembly));
                };

                options.MigrateDatabase = config.GetSection("EntityFramework").GetValue<bool>("MigrateDatabase");
                options.SeedExampleData = config.GetSection("EntityFramework").GetValue<bool>("SeedExampleData");
                options.SeedExampleDataPath = Path.Combine(".", "Config");

                services.AddDefaultStoreInitializer(options);
            });

            builder.Populate(services);
        }
    }
}
