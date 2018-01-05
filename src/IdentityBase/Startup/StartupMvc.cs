// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Razor;

    public static class StartupMvc
    {
        public static void AddMvc(
            this IServiceCollection services,
            ApplicationOptions appOptions,
            IHostingEnvironment environment)
        {
            services
                .AddRouting((options) =>
                {
                    options.LowercaseUrls = true;
                });

            services
                .AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(
                        new ThemeViewLocationExpander(appOptions.ThemePath)
                    );
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.ReplaceControllerFeatureProvider(
                        new WebControllerFeatureProvider(appOptions));
                });
        }
    }
}
