// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using ServiceBase.Localization;
    using ServiceBase.Resources;

    public static class StartupLocalization
    {
        /*
        services.AddJsonFileBasedLocalization(options =>
        {
            options.DefaultCulture = this._applicationOptions.DefaultCulture;
        }, options =>
        {
            options.RequestCultureProviders = new[]
            {
                new RequestCultureProvider()
            };
        });
        */

        public static void AddLocalization(
           this IServiceCollection services,
           ApplicationOptions appOptions,
           IHostingEnvironment environment)
        {
            services.TryAdd<IResourceStore,
                InMemoryResourceStore>(ServiceLifetime.Scoped);

            services.TryAdd<IStringLocalizerFactory,
                StringLocalizerFactory>(ServiceLifetime.Singleton);

            services.TryAdd<IStringLocalizer,
                StringLocalizer>(ServiceLifetime.Singleton);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                IServiceProvider provider = services.BuildServiceProvider();

                IResourceStore resourceStore =
                    provider.GetRequiredService<IResourceStore>();

                IEnumerable<string> cultures =
                    resourceStore.GetAllLocalizationCulturesAsync().Result;

                options.DefaultRequestCulture =
                    new RequestCulture(appOptions.DefaultCulture);

                options.SupportedCultures =
                options.SupportedUICultures =
                     cultures.Select(s => new CultureInfo(s)).ToList();

                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders
                    .Add(new IdentityBase.RequestCultureProvider());
            });
        }

        public static void UseLocalization(this IApplicationBuilder app)
        {
            IOptions<RequestLocalizationOptions> options = app
                .ApplicationServices
                .GetService<IOptions<RequestLocalizationOptions>>();

            app.UseRequestLocalization(options.Value);
        }
    }
}
