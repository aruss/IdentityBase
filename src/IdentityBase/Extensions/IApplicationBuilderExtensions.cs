// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using IdentityBase.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class IApplicationBuilderExtensions
    {
        public static void AddEmbeddedWebApi(
            this IApplicationBuilder appBuilder)
        {
            appBuilder.AddEmbedded("/api",
                "IdentityBase.WebApi.Startup, IdentityBase.WebApi");
        }

        public static void AddEmbeddedAdmin(
            this IApplicationBuilder appBuilder)
        {
            appBuilder.AddEmbedded("/api",
                "IdentityBase.Admin.Startup, IdentityBase.Admin");
        }

        public static void AddEmbedded(
            this IApplicationBuilder appBuilder,
            PathString path,
            string startupTypeName)
        {
            IConfiguration config = appBuilder.ApplicationServices
                .GetRequiredService<IConfiguration>();

            IHostingEnvironment environment = appBuilder.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            ILoggerFactory loggerFactory = appBuilder.ApplicationServices
                .GetRequiredService<ILoggerFactory>();

            IStartup startup = CreateStartupInstance(
                startupTypeName,
                config,
                environment,
                loggerFactory);

            appBuilder.MapStartup(
                path,
                environment,
                config,
                (services) =>
                {
                    startup.ConfigureServices(services);
                },
                (app) =>
                {
                    startup.Configure(app);
                });
        }

        private static IStartup CreateStartupInstance(
            string startupTypeName,
           IConfiguration configuration,
           IHostingEnvironment environment,
           ILoggerFactory loggerFactory)
        {
            Type type = Type.GetType(startupTypeName);

            if (type == null)
            {
                throw new ArgumentNullException(
                    "Cannot load " + startupTypeName);
            }

            return (IStartup)Activator.CreateInstance(
                type,
                configuration,
                environment,
                loggerFactory);
        }
    }
}
