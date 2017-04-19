using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.IdentityServer.Public.EntityFramework;
using System;
using System.Linq; 
using System.IO;
using System.Reflection;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupDataLayer
    {
        public static void AddDataLayer(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            if (config.GetChildren().Any(x => x.Key == "EntityFramework"))
            {
                services.AddEntityFrameworkStores((options) =>
                {   
                    // https://docs.microsoft.com/en-us/ef/core/providers/
                    if (config.GetChildren().Any(x => x.Key == "EntityFramework:SqlServer"))
                    {
                        var migrationsAssembly = typeof(IServiceCollectionExtensions).GetTypeInfo().Assembly.GetName().Name;
                        options.DbContextOptions = (builder) =>
                        {
                            builder.UseSqlServer(config["EntityFramework:SqlServer:ConnectionString"], o => o.MigrationsAssembly(migrationsAssembly));
                        };
                    }
                    else if (config.GetChildren().Any(x => x.Key == "EntityFramework:Npgsql"))
                    {
                        var migrationsAssembly = typeof(IServiceCollectionExtensions).GetTypeInfo().Assembly.GetName().Name;
                        options.DbContextOptions = (builder) =>
                        {
                            builder.UseNpgsql(config["EntityFramework:Npgsql:ConnectionString"], o => o.MigrationsAssembly(migrationsAssembly));
                        };
                    }
                    else
                    {
                        options.DbContextOptions = (builder) =>
                        {
                            builder.UseInMemoryDatabase();
                        };
                    }

                    options.MigrateDatabase = config.GetSection("EntityFramework").GetValue<bool>("MigrateDatabase");
                    options.SeedExampleData = config.GetSection("EntityFramework").GetValue<bool>("SeedExampleData");
                    options.SeedExampleDataPath = Path.Combine(environment.ContentRootPath, "Config");

                    services.AddDefaultStoreInitializer(options);
                });
            }
            /*else if (String.IsNullOrWhiteSpace(config["Dapper"]))
            {
                // Do dapper magic here 
            }*/
            else
            {
                throw new Exception("Store configuration not present");
            }
        }
    }
}
