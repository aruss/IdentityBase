namespace IdentityBase.EntityFramework.MySql
{
    using System;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;

    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services
                .BuildServiceProvider();

            IConfiguration config = serviceProvider
                .GetService<IConfiguration>();

            services.AddEntityFrameworkStores((options) =>
            {
                string migrationsAssembly =
                    typeof(ConfigureServicesAction)
                    .GetTypeInfo().Assembly.GetName().Name;

                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseMySql(
                        config["EntityFramework:MySql:ConnectionString"],
                        o => o.MigrationsAssembly(migrationsAssembly)
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });
        }
    }
}