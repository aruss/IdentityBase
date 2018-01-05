// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Net.Http;
    using IdentityBase.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// <see cref="IApplicationBuilder"/> extension methods.
    /// </summary>
    public static partial class IApplicationBuilderExtensions
    {
        public static void AddEmbeddedWebApi(
            this IApplicationBuilder appBuilder,
            Func<HttpMessageHandler> messageHandlerFactory = null)
        {
            appBuilder.AddEmbedded(
                "/api",
                "IdentityBase.WebApi.Startup, IdentityBase.WebApi",
                messageHandlerFactory);
        }

        public static void AddEmbeddedAdmin(
            this IApplicationBuilder appBuilder,
            Func<HttpMessageHandler> messageHandlerFactory = null)
        {
            appBuilder.AddEmbedded(
                "/admin",
                "IdentityBase.Admin.Startup, IdentityBase.Admin",
                messageHandlerFactory);
        }

        public static void AddEmbedded(
            this IApplicationBuilder appBuilder,
            PathString path,
            string startupTypeName,
            Func<HttpMessageHandler> messageHandlerFactory = null)
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
                loggerFactory,
                messageHandlerFactory);

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
           ILoggerFactory loggerFactory,
           Func<HttpMessageHandler> messageHandlerFactory = null)
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
                loggerFactory,
                messageHandlerFactory);
        }
    }
}
