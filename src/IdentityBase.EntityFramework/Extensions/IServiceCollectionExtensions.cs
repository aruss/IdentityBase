namespace IdentityBase.EntityFramework
{
    using System;
    using IdentityBase.EntityFramework.DbContexts;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Services;
    using IdentityBase.EntityFramework.Stores;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class IServiceCollectionExtensions
    {
        public static void AddEntityFrameworkStores(
            this IServiceCollection services,
            Action<EntityFrameworkOptions> configure = null)
        {
            // services.Configure<EntityFrameworkOptions>(configure);
            var options = new EntityFrameworkOptions();
            configure?.Invoke(options);

            services.AddEntityFrameworkStores(options);
        }

        public static void AddEntityFrameworkStores(
            this IServiceCollection services,
            EntityFrameworkOptions options)
        {
            // TODO: add only if migration is activated
            services
                .AddDbContext<MigrationDbContext>(options.DbContextOptions);

            services.AddConfigurationStore(options);
            services.AddOperationalStore(options);
            services.AddUserAccountStore(options);

            services.AddSingleton(options);
        }
        
        internal static void AddConfigurationStore(
            this IServiceCollection services,
            EntityFrameworkOptions options)
        {
            services.AddDbContext<ConfigurationDbContext>(
                options.DbContextOptions);

            services.AddScoped<IConfigurationDbContext,
                ConfigurationDbContext>();

            services.AddTransient<IClientStore, ClientStore>();
            services.AddTransient<IResourceStore, ResourceStore>();
            services.AddTransient<ICorsPolicyService, CorsPolicyService>();
        }

        internal static void AddOperationalStore(
            this IServiceCollection services,
            EntityFrameworkOptions options)
        {
            services.AddDbContext<PersistedGrantDbContext>(
                options.DbContextOptions);

            services.AddScoped<IPersistedGrantDbContext,
                PersistedGrantDbContext>();

            services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

            if (options.EnableTokenCleanup)
            {
                services.AddSingleton<IHostedService,
                    TokenCleanupHostedService>();
            }
        }

        internal static void AddUserAccountStore(
            this IServiceCollection services,
            EntityFrameworkOptions options)
        {
            services.AddDbContext<UserAccountDbContext>(
                options.DbContextOptions);

            services.AddScoped<IUserAccountDbContext, UserAccountDbContext>();
            services.AddTransient<IUserAccountStore, UserAccountStore>();
        }
    }
}