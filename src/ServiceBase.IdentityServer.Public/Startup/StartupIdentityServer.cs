using IdentityServer4.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.IdentityServer.Services;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupIdentityServer
    {
        public static void AddIdentityServer(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            var builder = services.AddIdentityServer((options) =>
            {
                // TODO: set configuration from config file 

                //options.RequireSsl = false;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ConsentUrl = "/consent";
                options.UserInteraction.ErrorUrl = "/error";

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
                builder.AddDeveloperSigningCredential();
            }
            else
            {

                var cert = new X509Certificate2(Path.Combine(
                    environment.ContentRootPath, "config/idsvr3test.pfx"), "idsrv3test");

                builder.AddSigningCredential(cert);

                /*builder.AddSigningCredential("98D3ACF057299C3745044BE918986AD7ED0AD4A2",
                    StoreLocation.CurrentUser, nameType: NameType.Thumbprint);*/
            }
        }
    }
}
