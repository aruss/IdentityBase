// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework
{
    using System.Threading;
    using IdentityBase.EntityFramework.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static partial class IApplicationBuilderExtensions
    {
        public static void WaitForDatabase(this IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                IConfigurationDbContext dbContext = serviceScope
                    .ServiceProvider
                    .GetService<IConfigurationDbContext>();

                ILogger logger = serviceScope
                    .ServiceProvider
                    .GetService<ILoggerFactory>()
                    .CreateLogger(typeof(IApplicationBuilderExtensions));

                DatabaseFacade database = (dbContext as DbContext).Database;

                do
                {
                    try
                    {
                        if (database.CanConnect())
                        {
                            break;
                        }

                        logger.LogWarning("Cannot connect to database");
                    }
                    catch (System.Exception ex)
                    {
                        logger.LogWarning(ex.Message);
                    }

                    Thread.Sleep(5000);
                } while (true);
            }
        }
    }
}
