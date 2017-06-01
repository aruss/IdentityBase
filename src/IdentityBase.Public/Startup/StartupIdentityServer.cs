using IdentityBase.Configuration;
using IdentityBase.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.Events;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace IdentityBase.Public
{
    public static class StartupIdentityServer
    {
        public static void AddIdentityServer(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            var eventOptions = config.GetSection("Events").Get<EventOptions>() ?? new EventOptions();
            var appOptions = config.GetSection("App").Get<ApplicationOptions>() ?? new ApplicationOptions();
            
            var builder = services.AddIdentityServer((options) =>
            {
                config.GetSection("IdentityServer").Bind(options); 
                
                options.Events.RaiseErrorEvents = eventOptions.RaiseErrorEvents;
                options.Events.RaiseFailureEvents = eventOptions.RaiseFailureEvents;
                options.Events.RaiseInformationEvents = eventOptions.RaiseInformationEvents;
                options.Events.RaiseSuccessEvents = eventOptions.RaiseSuccessEvents;

                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ConsentUrl = "/consent";
                options.UserInteraction.ErrorUrl = "/error";
            
                options.Cors.CorsPolicyName = "CorsPolicy"; 

                options.Authentication.FederatedSignOutPaths.Add("/signout-oidc");
                options.Authentication.FederatedSignOutPaths.Add("/signout-callback-aad");
                options.Authentication.FederatedSignOutPaths.Add("/signout-callback-idsrv");
                options.Authentication.FederatedSignOutPaths.Add("/signout-callback-adfs");                

            }).AddProfileService<ProfileService>()
             .AddSecretParser<ClientAssertionSecretParser>()
             .AddSecretValidator<PrivateKeyJwtSecretValidator>();

            // AppAuth enabled redirect URI validator
            services.AddTransient<IRedirectUriValidator, StrictRedirectUriValidatorAppAuth>();

            if (environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential(Path.Combine(appOptions.TempFolder, "tempkey.rsa")); 
            }
            else
            {
                if (config.ContainsSection("IdentityServer"))
                {
                    var section = config.GetSection("IdentityServer"); 
                    if (section.ContainsSection("SigningCredentialFromPfx"))
                    {
                        var filePath = section.GetValue<string>("SigningCredentialFromPfx:Path");
                        if (!Path.IsPathRooted(filePath))
                        {
                            filePath = Path.Combine(environment.ContentRootPath, filePath);
                        }
                        if (!File.Exists(filePath))
                        {
                            throw new FileNotFoundException("Signing certificate file not found", filePath); 
                        }
                        var password = section.GetValue<string>("SigningCredentialFromPfx:Password");
                        builder.AddSigningCredential(new X509Certificate2(filePath, password));
                    }
                    if (section.ContainsSection("SigningCredentialFromStore"))
                    {
                        throw new NotImplementedException();
                        // builder.AddSigningCredential("98D3ACF057299C3745044BE918986AD7ED0AD4A2", StoreLocation.LocalMachine, nameType: NameType.Thumbprint);
                    }
                    else
                    {
                        builder.AddTemporarySigningCredential();
                    }
                }
                else
                {
                    builder.AddTemporarySigningCredential();
                }                
            }
        }
    }
}
