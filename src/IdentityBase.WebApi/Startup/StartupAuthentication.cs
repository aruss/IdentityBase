// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi
{
    using System.Reflection;
    using IdentityBase.Configuration;
    using IdentityBase.WebApi.Actions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;

    public static class StartupAuthentication
    {
        public static void AddAuthentication(
            this IServiceCollection services,
            WebApiOptions webApiOptions)
        {
            Assembly assembly = typeof(WebApiModule)
               .GetTypeInfo().Assembly;

            services
                .AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = webApiOptions.AutorityUrl;

                    // TODO: extract to string extension
                    options.RequireHttpsMetadata =
                    webApiOptions.AutorityUrl.IndexOf("https") > -1;

                    // TODO: move to constants
                    options.ApiName = WebApiConstants.ApiName;
                    options.ApiSecret = webApiOptions.ApiSecret;
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
