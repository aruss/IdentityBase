namespace IdentityBase.EntityFramework.InMemory
{
    using System;
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
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder
                        .UseInMemoryDatabase("Put_value_from_config_here");
                };

                config.GetSection("EntityFramework").Bind(options);
            });
        }
    }
}