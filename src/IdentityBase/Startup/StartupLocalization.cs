// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Collections.Generic;
    using System.Globalization;
    using IdentityBase.Configuration;
    using IdentityBase.Localization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using ServiceBase.Extensions;

    public static class StartupLocalization
    {
        public static void AddLocalization(
           this IServiceCollection services,
           ApplicationOptions appOptions,
           IHostingEnvironment environment)
        {
            services.TryAdd<IStringLocalizerFactory,
                JsonStringLocalizerFactory>(ServiceLifetime.Singleton);

            services.TryAdd<IStringLocalizer,
                JsonStringLocalizer>(ServiceLifetime.Singleton);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture =
                    new RequestCulture(appOptions.DefaultCulture);
                               
                options.SupportedCultures =
                options.SupportedUICultures = new List<CultureInfo>
                {
                    // TODO: read from ThemeHelper ...
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };

                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders
                    .Add(new IdentityBaseRequestCultureProvider());
            });
        }

        public static void UseLocalization(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices
                .GetService<IOptions<RequestLocalizationOptions>>();

            app.UseRequestLocalization(options.Value);
        }
    }
}
