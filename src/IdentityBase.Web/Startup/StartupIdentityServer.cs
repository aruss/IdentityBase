// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using IdentityBase.Configuration;
    using IdentityBase.Services;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Events;
    using ServiceBase.Extensions;
    
    // TODO: evaluate this http://docs.identityserver.io/en/release/topics/discovery.html

    // https://github.com/IdentityServer/IdentityServer4/blob/dev/src/Host/Startup.cs
    public static class StartupIdentityServer
    {
        public static void AddIdentityServer(
            this IServiceCollection services,
            IConfiguration config,
            ILogger logger,
            IHostingEnvironment environment)
        {
            EventOptions eventOptions = config.GetSection("Events")
                .Get<EventOptions>() ?? new EventOptions();

            ApplicationOptions appOptions = config.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();

            // http://docs.identityserver.io/en/latest/reference/options.html
            IIdentityServerBuilder builder =
                services.AddIdentityServer((options) =>
            {
                config.GetSection("IdentityServer").Bind(options);

                options.Events.RaiseErrorEvents =
                    eventOptions.RaiseErrorEvents;

                options.Events.RaiseFailureEvents =
                    eventOptions.RaiseFailureEvents;

                options.Events.RaiseInformationEvents =
                    eventOptions.RaiseInformationEvents;

                options.Events.RaiseSuccessEvents =
                    eventOptions.RaiseSuccessEvents;

                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ConsentUrl = "/consent";
                options.UserInteraction.ErrorUrl = "/error";

            })
            .AddProfileService<ProfileService>()
            .AddSecretParser<JwtBearerClientAssertionSecretParser>()
            .AddSecretValidator<PrivateKeyJwtSecretValidator>()
            .AddRedirectUriValidator<StrictRedirectUriValidatorAppAuth>()
            .AddSigningCredential(environment, config, appOptions)
            .AddJwtBearerClientAuthentication()
            .AddAppAuthRedirectUriValidator();

        }

        private static IIdentityServerBuilder AddSigningCredential(
              this IIdentityServerBuilder builder,
              IHostingEnvironment environment,
              IConfiguration config,
              ApplicationOptions appOptions)
        {
            if (environment.IsDevelopment())
            {
                string path = Path.Combine(appOptions.TempPath
                    .GetFullPath(environment.ContentRootPath), "tempkey.rsa");

                builder.AddDeveloperSigningCredential(false, path);

                return builder;
            }

            if (!config.ContainsSection("IdentityServer"))
            {
                throw new ApplicationException(
                    "Missing IdentityServer configuration");
            }

            IConfigurationSection section = config
                .GetSection("IdentityServer");

            if (section.ContainsSection("SigningCredentialFromPfx"))
            {
                string filePath = section
                           .GetValue<string>("SigningCredentialFromPfx:Path")
                           .GetFullPath(environment.ContentRootPath);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(
                        "Signing certificate file not found.",
                        filePath
                    );
                }

                string password = section.GetValue<string>(
                    "SigningCredentialFromPfx:Password");

                builder.AddSigningCredential(
                    new X509Certificate2(filePath, password));
                return builder;
            }

            if (section.ContainsSection("SigningCredentialFromStore"))
            {
                throw new NotImplementedException();
            }
            
            return builder;
        }
    }
}