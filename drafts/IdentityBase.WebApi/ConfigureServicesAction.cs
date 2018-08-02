// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi
{
    using System;
    using System.Reflection;
    using IdentityBase.Configuration;
    using IdentityBase.WebApi.Actions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;
    using ServiceBase.Extensions;
    using ServiceBase.Plugins;

    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services
                .BuildServiceProvider();

            IConfiguration configuration = serviceProvider
                .GetService<IConfiguration>();

            Assembly assembly = typeof(ConfigureServicesAction)
                .GetTypeInfo().Assembly;

            WebApiOptions webApiOptions = configuration.GetSection("WebApi")
                .Get<WebApiOptions>() ?? new WebApiOptions();

            services
                .AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.ApiName = WebApiConstants.ApiName;
                    options.ApiSecret = webApiOptions.ApiSecret;
                    options.Authority = webApiOptions.AutorityUrl;

                    options.RequireHttpsMetadata =
                        webApiOptions.AutorityUrl.IsSecureUrl();

                    // TODO: add custom message handler for test cases 
                    // Set message handler, mostly used for integration tests
                    //if (messageHandlerFactory != null)
                    //{
                    //    HttpMessageHandler messageHandler =
                    //        messageHandlerFactory();
                    //
                    //    options.IntrospectionDiscoveryHandler = messageHandler;
                    //    options.IntrospectionDiscoveryHandler = messageHandler;
                    //    options.JwtBackChannelHandler = messageHandler;
                    //}
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddScopePolicies<WebApiController>(
                        webApiOptions.AutorityUrl,
                        assembly: assembly,
                        fromReferenced: true
                    );
                });
        }
    }
}
