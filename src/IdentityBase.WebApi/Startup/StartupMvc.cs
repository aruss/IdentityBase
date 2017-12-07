// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi
{
    using System.Reflection;
    using IdentityBase.Configuration;
    using IdentityBase.WebApi.Actions;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;
    using ServiceBase.Mvc;

    public static class StartupMvc
    {
        public static void AddMvc(
            this IServiceCollection services,
            WebApiOptions webApiOptions)
        {
            Assembly assembly = typeof(StartupMvc)
               .GetTypeInfo().Assembly;

            services
                .AddRouting((options) =>
                {
                    options.LowercaseUrls = true;
                });
            
            services
                .AddMvc(mvcOptions =>
                {
                    mvcOptions.OutputFormatters
                        .AddDefaultJsonOutputFormatter();
                })
                .AddApplicationPart(assembly)
                .AddControllersAsServices()
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Clear();
                    manager.FeatureProviders.Add(
                        new TypedControllerFeatureProvider<WebApiController>());
                });
        }
    }
}