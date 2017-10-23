namespace IdentityBase.Public.EntityFramework.SqlServer
{
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class SqlServerModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies
        /// </summary>
        /// <param name="builder">The builder through which components can
        /// be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            ServiceCollection services = new ServiceCollection();
            IConfiguration config = Current.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                string migrationsAssembly =
                    typeof(SqlServerModule)
                    .GetTypeInfo().Assembly.GetName().Name;

                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseSqlServer(
                        config["EntityFramework:SqlServer:ConnectionString"],
                        o => o.MigrationsAssembly(migrationsAssembly)
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });

            builder.Populate(services);
        }
    }
}