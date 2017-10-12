namespace IdentityBase.Public.EntityFramework
{
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class NpgsqlModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            ServiceCollection services = new ServiceCollection();
            IConfiguration config = Current.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                string migrationsAssembly =
                    typeof(IServiceCollectionExtensions)
                    .GetTypeInfo().Assembly.GetName().Name;

                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseNpgsql(
                        config["EntityFramework:Npgsql:ConnectionString"],
                        o => o.MigrationsAssembly(migrationsAssembly)
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });

            builder.Populate(services);
        }
    }
}