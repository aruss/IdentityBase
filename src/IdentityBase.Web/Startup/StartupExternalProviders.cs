// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using IdentityServer4;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class StartupExternalProviders
    {
        public static void AddExternalProviders(
           this IServiceCollection services,
           IConfiguration config,
                 ILogger logger)
        {
            if (!config.ContainsSection("ThirdPartyAuth"))
            {
                logger.LogInformation(
                  "No third party authentication provides registered");

                return;
            }

            // configures the OpenIdConnect handlers to persist the state
            // parameter into the server-side IDistributedCache.
            // services.AddOidcStateDataFormatterCache("aad","demoidsrv");

            AuthenticationBuilder authBuilder = services.AddAuthentication();

            IConfigurationSection authConfig =
                config.GetSection("ThirdPartyAuth");
            
            if (authConfig.ContainsSection("Google"))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;

                    options.ClientId = authConfig["Google:ClientId"];
                    options.ClientSecret = authConfig["Google:ClientSecret"];
                });
            }

            if (authConfig.ContainsSection("Facebook"))
            {
                authBuilder.AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;

                    options.ClientId = authConfig["Facebook:ClientId"];
                    options.ClientSecret = authConfig["Facebook:ClientSecret"];
                });
            }

            if (authConfig.ContainsSection("Microsoft"))
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityServerConstants
                        .ExternalCookieAuthenticationScheme;

                    options.ClientId = authConfig["Microsoft:ClientId"];
                    options.ClientSecret = authConfig["Microsoft:ClientSecret"];
                });
            }

        }
    }
}
