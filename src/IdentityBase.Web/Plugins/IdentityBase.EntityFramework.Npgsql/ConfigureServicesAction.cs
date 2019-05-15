// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Npgsql
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

                IConfigurationSection efConfig =
                    config.GetSection("EntityFramework");

                options.DbContextOptions = (dbBuilder) =>
                {
                    NpgsqlEfOptions npgsqlOptions =
                        efConfig.GetSection("Npgsql").Get<NpgsqlEfOptions>()
                        ?? new NpgsqlEfOptions();

                    dbBuilder.UseNpgsql(
                        npgsqlOptions.ConnectionString,
                        o =>
                        {
                            o.MigrationsAssembly(migrationsAssembly);
                            o.EnableRetryOnFailure(
                                maxRetryCount: npgsqlOptions.MaxRetryCount,
                                maxRetryDelay: TimeSpan
                                    .FromSeconds(npgsqlOptions.MaxRetryDelay),
                                errorCodesToAdd: null);
                        }
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });
        }
    }
}