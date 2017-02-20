using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceBase.IdentityServer.Crypto;
using ServiceBase.IdentityServer.EntityFramework;
using ServiceBase.IdentityServer.Extensions;
using ServiceBase.IdentityServer.Services;
using ServiceBase.Notification.Email;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ServiceBase.IdentityServer.Public.IntegrationTests
{
    public class TestStartup
    {
        public TestStartup(IHostingEnvironment environment)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddIdentityServer((options) =>
            {
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
            })
                .AddTemporarySigningCredential()
                .AddProfileService<ProfileService>()
                .AddSecretParser<ClientAssertionSecretParser>()
                .AddSecretValidator<PrivateKeyJwtSecretValidator>()
                .AddTemporarySigningCredential();

            // Register mocked email service in case none of the tests registered one already
            if (!services.Any(c => c.ServiceType == typeof(IEmailService)))
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);
            }

            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();

            #region Entity Framework Store Layer

            services.AddEntityFrameworkStores((options) =>
            {
                options.MigrateDatabase = false;
                options.SeedExampleData = false;
            });

            // Register default store initializer in case none of the tests registered one already
            if (!services.Any(c => c.ServiceType == typeof(IStoreInitializer)))
            {
                services.AddTransient<IStoreInitializer, TestStoreInitializer>();
            }

            #endregion Entity Framework Store Layer

            services
              .AddMvc()
              .AddRazorOptions(razor =>
              {
                  razor.ViewLocationExpanders.Add(new Public.UI.CustomViewLocationExpander());
              });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseExceptionHandler("/error");
            app.UseIdentityServer();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.InitializeStores();
        }
    }
}