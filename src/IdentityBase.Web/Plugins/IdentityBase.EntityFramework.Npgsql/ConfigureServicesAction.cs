namespace IdentityBase.EntityFramework.Npgsql
{
    using System;
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Mvc.Plugins;

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
                    dbBuilder.UseNpgsql(
                        config["EntityFramework:Npgsql:ConnectionString"],
                        o => o.MigrationsAssembly(migrationsAssembly)
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });
        }
    }   
}