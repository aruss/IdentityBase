using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.IdentityServer.Public.EntityFramework.DbContexts;
using ServiceBase.IdentityServer.Public.EntityFramework.Interfaces;
using ServiceBase.IdentityServer.Public.EntityFramework.Options;
using ServiceBase.IdentityServer.Public.EntityFramework.Services;
using ServiceBase.IdentityServer.Public.EntityFramework.Stores;
using ServiceBase.IdentityServer.Services;
using System;

namespace ServiceBase.IdentityServer.Public.EntityFramework
{
    public static class IServiceCollectionExtensions
    {
        public static void AddEntityFrameworkStores(this IServiceCollection services, Action<EntityFrameworkOptions> configure = null)
        {
            // services.Configure<EntityFrameworkOptions>(configure);
            var options = new EntityFrameworkOptions();
            configure?.Invoke(options);

            services.AddEntityFrameworkStores(options);
        }

        public static void AddEntityFrameworkStores(this IServiceCollection services, EntityFrameworkOptions options)
        {
            services.AddConfigurationStore(options);
            services.AddOperationalStore(options);
            services.AddUserAccountStore(options);

            services.AddSingleton(options);
        }

        public static void AddDefaultStoreInitializer(this IServiceCollection services, EntityFrameworkOptions options)
        {
            services.AddDbContext<DefaultDbContext>(options.DbContextOptions);
            services.AddScoped<ConfigurationDbContext>();
            services.AddTransient<IStoreInitializer, DefaultStoreInitializer>();
        }

        internal static void AddConfigurationStore(this IServiceCollection services, EntityFrameworkOptions options)
        {
            services.AddDbContext<ConfigurationDbContext>(options.DbContextOptions);
            services.AddScoped<IConfigurationDbContext, ConfigurationDbContext>();

            services.AddTransient<IClientStore, ClientStore>();
            services.AddTransient<IResourceStore, ResourceStore>();
            services.AddTransient<ICorsPolicyService, CorsPolicyService>();
        }

        internal static void AddOperationalStore(this IServiceCollection services, EntityFrameworkOptions options)
        {
            services.AddDbContext<PersistedGrantDbContext>(options.DbContextOptions);
            services.AddScoped<IPersistedGrantDbContext, PersistedGrantDbContext>();
            services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

            if (options.CleanupTokens)
            {
                services.AddSingleton<TokenCleanup>();
            }
        }

        internal static void AddUserAccountStore(this IServiceCollection services, EntityFrameworkOptions options)
        {
            services.AddDbContext<UserAccountDbContext>(options.DbContextOptions);
            services.AddScoped<IUserAccountDbContext, UserAccountDbContext>();
            services.AddTransient<IUserAccountStore, UserAccountStore>();
        }
    }
}