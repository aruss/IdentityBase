// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
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

    public class DummyStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name]
        {
            get
            {
                return new LocalizedString(name, name);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                return new LocalizedString(name, name); 
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new System.NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DummyStringLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource)
        {
            return new DummyStringLocalizer(); 
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new DummyStringLocalizer();
        }
    }

    public static class StartupLocalization
    {
        public static void AddLocalization(
           this IServiceCollection services,
           ApplicationOptions appOptions,
           IHostingEnvironment environment)
        {
            //services.TryAdd<IStringLocalizerFactory,
            //    JsonStringLocalizerFactory>(ServiceLifetime.Singleton);

            //services.TryAdd<IStringLocalizer,
            //   JsonStringLocalizer>(ServiceLifetime.Singleton);

            services.TryAdd<IStringLocalizerFactory,
                DummyStringLocalizerFactory>(ServiceLifetime.Singleton);

            services.TryAdd<IStringLocalizer,
               DummyStringLocalizer>(ServiceLifetime.Singleton);

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
