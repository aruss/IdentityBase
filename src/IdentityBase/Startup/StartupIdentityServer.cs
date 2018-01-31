// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Events;

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
            
            var builder = services.AddIdentityServer((options) =>
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

                // options.Cors.CorsPolicyName = "CorsPolicy";

                // options.Authentication
                //     .FederatedSignOutPaths.Add("/signout-oidc");
                // 
                // options.Authentication
                //     .FederatedSignOutPaths.Add("/signout-callback-aad");
                // 
                // options.Authentication
                //     .FederatedSignOutPaths.Add("/signout-callback-idsrv");
                // 
                // options.Authentication
                //     .FederatedSignOutPaths.Add("/signout-callback-adfs");
            })
            .AddProfileService<ProfileService>()
            .AddSecretParser<JwtBearerClientAssertionSecretParser>()
            .AddSecretValidator<PrivateKeyJwtSecretValidator>()
            .AddRedirectUriValidator<StrictRedirectUriValidatorAppAuth>();

            if (environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential(
                    false,
                    Path.Combine(
                        appOptions.TempPath.GetFullPath(
                            environment.ContentRootPath),
                        "tempkey.rsa"
                    )
                );
            }
            else
            {
                if (config.ContainsSection("IdentityServer"))
                {
                    IConfigurationSection section =
                        config.GetSection("IdentityServer");

                    if (section.ContainsSection("SigningCredentialFromPfx"))
                    {
                        string filePath = section
                            .GetValue<string>("SigningCredentialFromPfx:Path")
                            .GetFullPath(environment.ContentRootPath);

                        if (!File.Exists(filePath))
                        {
                            throw new FileNotFoundException(
                                "Signing certificate file not found",
                                filePath
                            );
                        }

                        string password = section.GetValue<string>(
                            "SigningCredentialFromPfx:Password");

                        builder.AddSigningCredential(
                            new X509Certificate2(filePath, password));
                    }

                    if (section.ContainsSection("SigningCredentialFromStore"))
                    {
                        throw new NotImplementedException();
                        // builder.AddSigningCredential("98D3ACF057299C3745044BE918986AD7ED0AD4A2",
                        // StoreLocation.LocalMachine, nameType: NameType.Thumbprint);
                    }
                    else
                    {
                        builder.AddDeveloperSigningCredential();
                    }
                }
                else
                {
                    builder.AddDeveloperSigningCredential();
                }
            }
        }
    }
}